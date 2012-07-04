/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using ChemProV.Core;
using ChemProV.PFD;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.StickyNote;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.PFD.Streams.PropertiesWindow.Chemical;
using ChemProV.PFD.Streams.PropertiesWindow.Heat;
using ChemProV.UI.DrawingCanvas.States;
using System.ComponentModel;

namespace ChemProV.UI.DrawingCanvas
{
    public delegate void PfdUpdatedEventHandler(object sender, PfdUpdatedEventArgs e);

    /// <summary>
    /// This is our drawing drawing_canvas, the thing that ProcessUnits and Streams are dragged onto.
    /// It controls everything that goes on with itself.
    /// </summary>
    public class DrawingCanvas : Canvas
    {
        public event PfdUpdatedEventHandler PfdUpdated = delegate { };
        public event EventHandler PfdChanging = delegate { };

        /// <summary>
        /// This is a debugging-oriented state tracker. When the CurrentState property is set to 
        /// some value, we check to see if our current state is non-null. If it is non-null then 
        /// the contract is that we let that state know that it's ending and we're going to a new 
        /// one. We do this by calling the state's StateEnding() function. Before we call we set 
        /// this to true and after the call returns we set it back to false.
        /// This allows us to ensure that the StateEnding() function doesn't try to set 
        /// CurrentState again. States are not allowed to set the drawing canvas's state from 
        /// their StateEnding() functions.
        /// In theory this could be enforced using reflection and eliminate the need for this 
        /// variable, but that's something that I'll look into later.
        /// </summary>
        private bool m_endingAState = false;

        /// <summary>
        /// Used to prevent recursive sets to CurrentState
        /// </summary>
        private bool m_settingCurrentState = false;

        private Core.Workspace m_workspace = null;

        #region States

        /// <summary>
        /// This is a variable that saves the current state.
        /// </summary>
        private IState m_currentState = null;

        /// <summary>
        /// Gets or sets the current state of the drawing canvas. State objects process mouse-input on 
        /// the drawing canvas and perform actions accordingly.
        /// </summary>
        public IState CurrentState
        {
            get { return m_currentState; }
            set
            {
                if (m_settingCurrentState)
                {
                    return;
                }
                m_settingCurrentState = true;
                
                // Ensure that the popup menu is hidden, provided we aren't going into a menu state
                if (!(value is MenuState))
                {
                    Core.App.ClosePopup();
                }
                
                // Start with a check to see if we're currently ending a state
                if (m_endingAState)
                {
                    m_settingCurrentState = false;
                    throw new InvalidOperationException(
                        "Objects that implement IState are NOT permitted to set the " +
                        "DrawingCanvas.CurrentState property when in their StateEnding() method.");
                }
                
                // The contract for items that implement IState is that when we're switching from 
                // them to something else, we let them know that their state is ending
                if (null != m_currentState &&
                    !object.ReferenceEquals(value, m_currentState))
                {
                    m_endingAState = true;
                    m_currentState.StateEnding();
                    m_endingAState = false;
                }

                m_currentState = value;
                m_settingCurrentState = false;
            }
        }

        #endregion States

        #region Properties

        private bool isReadOnly = false;

        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set { isReadOnly = value; }
        }

        /// <summary>
        /// This is used to decided if main page should route key down to drawing drawing_canvas
        /// </summary>
        private bool hasFocus;

        public bool HasFocus1
        {
            get { return hasFocus; }
            set { hasFocus = value; }
        }

        /// <summary>
        /// This stores the currently selected element
        /// </summary>
        private ICanvasElement selectedElement;

        public ICanvasElement SelectedElement
        {
            get { return selectedElement; }
            set
            {
                //This checks to see if we had an element selected if so tell it it is no longer selected
                IPfdElement oldvalue = selectedElement as IPfdElement;
                if (oldvalue != null)
                {
                    oldvalue.Selected = false;
                }

                //If we are selecting something tell it to be selected all PFDElements have an event when Selected
                //is changed and draw the boarder around themselves appropriately.
                if (value != null)
                {
                    if (value is IPfdElement)
                    {
                        (value as IPfdElement).Selected = true;
                    }
                    if (value is UserControl)
                    {
                        (value as UserControl).Focus();
                    }
                }

                selectedElement = value;
            }
        }

        public List<StickyNoteControl> ChildStickyNotes
        {
            get
            {
                var snElements = from c in this.Children where c is StickyNoteControl select c as StickyNoteControl;
                return new List<StickyNoteControl>(snElements);
            }
        }

        public List<IPfdElement> ChildIPfdElements
        {
            get
            {
                var ipfdElements = from c in this.Children where c is IPfdElement select c as IPfdElement;
                return new List<IPfdElement>(ipfdElements);
            }
        }

        #endregion Properties

        /// <summary>
        /// Raised whenever the drawing_canvas places a drawing tool
        /// </summary>
        public event EventHandler ToolPlaced = delegate { };
        public event EventHandler feedbackLabelEvent = delegate { };

        /// <summary>
        /// This is the constructor, we make the states we will be using and set the mouse events.
        /// </summary>
        public DrawingCanvas()
            : base()
        {
            // Set event listeners
            MouseEnter += new MouseEventHandler(MouseEnterHandler);
            MouseLeave += new MouseEventHandler(MouseLeaveHandler);
            MouseMove += new MouseEventHandler(MouseMoveHandler);
            MouseWheel += new MouseWheelEventHandler(MouseWheelHandler);
            MouseLeftButtonDown += new MouseButtonEventHandler(MouseLeftButtonDownHandler);
            MouseLeftButtonUp += new MouseButtonEventHandler(MouseLeftButtonUpHandler);
            MouseRightButtonDown += new MouseButtonEventHandler(MouseRightButtonDownHandler);
            MouseRightButtonUp += new MouseButtonEventHandler(MouseRightButtonUpHandler);
            SizeChanged += new SizeChangedEventHandler(DrawingCanvas_SizeChanged);
        }

        /// <summary>
        /// Resets the clipping region of the drawing drawing_canvas whenever it gets resized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RectangleGeometry rect = new RectangleGeometry();
            rect.Rect = new Rect(0, 0, ActualWidth, ActualHeight);
            Clip = rect;
        }

        /// <summary>
        /// This method gets the first child control that contains the specified point. If no 
        /// children contain the point then null is returned.
        /// </summary>
        /// <param name="location">Location, relative to this canvas</param>
        /// <param name="excludeMe">Object reference to ignore. Note that if the ONLY child at the 
        /// location is equal to this then null will be returned. This is useful for scenarios where 
        /// you might be dragging one object on top of the other. In this case obviously the child 
        /// you're your dragging is going to be at the location you specify, but you want to ignore 
        /// that child and look for anything else it might be getting dragged onto.</param>
        /// <returns>First child containing the point, or null if there are no such children</returns>
        public UIElement GetChildAt(Point location, object excludeMe)
        {
            foreach (UIElement uie in Children)
            {
                if (object.ReferenceEquals(excludeMe, uie) ||
                    System.Windows.Visibility.Collapsed == uie.Visibility)
                {
                    continue;
                }
                
                double w = (double)uie.GetValue(Canvas.ActualWidthProperty);
                double h = (double)uie.GetValue(Canvas.ActualHeightProperty);
                double x = (double)uie.GetValue(Canvas.LeftProperty);
                double y = (double)uie.GetValue(Canvas.TopProperty);

                if (location.X >= x && location.X < x + w &&
                    location.Y >= y && location.Y < y + h)
                {
                    return uie;
                }
            }

            // Coming here means we didn't find anything
            return null;
        }

        public UIElement GetChildAtIncludeStreams(Point location)
        {
            return GetChildAtIncludeStreams(location, null);
        }

        public UIElement GetChildAtIncludeStreams(Point location, params object[] excludeThese)
        {
            UIElement element = null;
            
            foreach (UIElement uie in Children)
            {
                // If it's in the exclusion list then ignore it
                if (null != excludeThese)
                {
                    if (Array.IndexOf<object>(excludeThese, uie) >= 0)
                    {
                        continue;
                    }
                }

                // If it's collapsed then ignore it
                if (System.Windows.Visibility.Collapsed == uie.Visibility)
                {
                    continue;
                }
                
                double w = (double)uie.GetValue(Canvas.ActualWidthProperty);
                double h = (double)uie.GetValue(Canvas.ActualHeightProperty);
                double x = (double)uie.GetValue(Canvas.LeftProperty);
                double y = (double)uie.GetValue(Canvas.TopProperty);

                if (location.X >= x && location.X < x + w &&
                    location.Y >= y && location.Y < y + h)
                {
                    // We found something at this location, but we need to check if we have another element 
                    // with a higher Z-index
                    if (null != element)
                    {
                        if ((int)uie.GetValue(Canvas.ZIndexProperty) >= (int)element.GetValue(Canvas.ZIndexProperty))
                        {
                            element = uie;
                        }
                    }
                    else
                    {
                        element = uie;
                    }
                }
            }

            // Check streams if we didn't get anything above
            if (null == element)
            {
                // We'll say any click within 4-pixels distance from the line is close enough
                double dist = 4.0;
                foreach (UIElement uie in Children)
                {
                    // If it's in the exclusion list then ignore it
                    if (null != excludeThese)
                    {
                        if (Array.IndexOf<object>(excludeThese, uie) >= 0)
                        {
                            continue;
                        }
                    }

                    PFD.Streams.AbstractStream stream = uie as PFD.Streams.AbstractStream;
                    if (null == stream)
                    {
                        continue;
                    }

                    MathCore.LineSegment ls = new MathCore.LineSegment(
                        new Point(stream.Stem.X1, stream.Stem.Y1),
                        new Point(stream.Stem.X2, stream.Stem.Y2));
                    if (ls.GetDistance(location) <= dist)
                    {
                        return stream;
                    }
                }
            }

            return element;
        }

        public ProcessUnitControl GetProcessUnitControl(Core.AbstractProcessUnit unit)
        {
            foreach (UIElement uie in Children)
            {
                if (!(uie is ProcessUnitControl))
                {
                    continue;
                }

                ProcessUnitControl pu = uie as ProcessUnitControl;
                if (object.ReferenceEquals(pu.ProcessUnit, unit))
                {
                    return pu;
                }
            }

            return null;
        }

        public int CountChildrenOfType(Type type)
        {
            int count = 0;
            foreach (UIElement uie in Children)
            {
                if (uie.GetType().Equals(type))
                {
                    count++;
                }
            }

            return count;
        }

        #region DrawingCanvasMouseEvents

        /// <summary>
        /// For all of the possible mouse actions we just call the handler for whatever state we are in.
        /// must set e.handled to true to stop multiple fires if we have layered objects.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseRightButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            hasFocus = true;
            e.Handled = true;
        }

        public void MouseRightButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            hasFocus = true;
            e.Handled = true;

            // Go ahead and select the item under the mouse
            object child = GetChildAtIncludeStreams(e.GetPosition(this));
            SelectedElement = child as ICanvasElement;
            if (child is DraggableStreamEndpoint)
            {
                SelectedElement = (child as DraggableStreamEndpoint).ParentStream;
            }

            // A right mouse button down implies that we need to flip to the menu state
            CurrentState = new UI.DrawingCanvas.States.MenuState(this, m_workspace);
            (m_currentState as MenuState).Show(e);
        }

        public bool IsStreamEndpoint(UIElement element)
        {
            foreach (UIElement uie in Children)
            {
                PFD.Streams.AbstractStream s = uie as PFD.Streams.AbstractStream;
                if (s != null)
                {
                    if (element == s.SourceDragIcon || element == s.DestinationDragIcon)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void MouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (null != m_currentState)
            {
                m_currentState.MouseLeftButtonUp(sender, e);
            }
            hasFocus = true;
            e.Handled = true;
        }

        public void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            hasFocus = true;

            // This canvas gets mouse events after any child element within it will 
            // get the events. We stop the event from going to higher parents of this 
            // canvas by setting Handled equal to true.

            // If the current state is non-null then send the mouse event to it
            if (null != m_currentState)
            {
                m_currentState.MouseLeftButtonDown(sender, e);
                return;
            }

            Core.App.ClosePopup();

            // If our current state is null, then we want to create an appropriate one.
            // But first we need to check to see if we've selected an element
            object childAtPos = GetChildAtIncludeStreams(e.GetPosition(this));
            SelectedElement = childAtPos as ICanvasElement;

            // If there is nothing where the mouse pointer is, then we leave the state 
            // null and return.
            if (null == childAtPos)
            {
                return;
            }

            // Otherwise we check to see if the selected element has its own mouse 
            // processing logic
            IState selectedObjState = childAtPos as IState;
            if (null != selectedObjState)
            {
                // Set the state
                m_currentState = selectedObjState;
            }
            else
            {
                // If the selected element does not have its own mouse processing logic then 
                // we create a MovingState object, which will drag around the selected element.
                m_currentState = MovingState.Create(this, m_workspace);
            }

            // Finish up by sending this mouse event to the current state
            if (null != m_currentState)
            {
                m_currentState.MouseLeftButtonDown(sender, e);
            }
        }

        public void MouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
        }

        public void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (null != m_currentState)
            {
                m_currentState.MouseMove(sender, e);
            }
        }

        public void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            if (null != m_currentState)
            {
                m_currentState.MouseLeave(sender, e);
            }
        }

        public void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            if (null != m_currentState)
            {
                m_currentState.MouseEnter(sender, e);
            }
        }

        #endregion DrawingCanvasMouseEvents

        #region IPfdMouseEvents

        /// <summary>
        /// Called whenever a user updates table data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TableDataChanged(object sender, TableDataChangedEventArgs e)
        {
            PFDModified();
        }

        public void TableDataChanging(object sender, EventArgs e)
        {
            PFDChanging();
        }

        #endregion IPfdMouseEvents

        /// <summary>
        /// Calling this function will cause the drawing_canvas to recalculate its height and width.
        /// Adds a 100px buffer on each side to allow for scrolling.
        /// </summary>
        public void UpdateCanvasSize()
        {
            double maxY = 0.0;
            double maxX = 0.0;
            foreach (UIElement child in Children)
            {
                maxX = Math.Max(maxX, Convert.ToDouble(child.GetValue(Canvas.LeftProperty)));
                maxY = Math.Max(maxY, Convert.ToDouble(child.GetValue(Canvas.TopProperty)));
            }
            if (
                    maxY > ActualHeight - SizeBuffer
                    ||
                    maxX + SizeBuffer < ActualHeight
                )
            {
                Height = maxY + SizeBuffer;
            }
            if (maxX > ActualWidth - 100.0
                ||
                maxX + SizeBuffer < ActualWidth
                )
            {
                Width = maxX + SizeBuffer;
            }
        }

        /// <summary>
        /// Gets how large of buffer we should allow for the edge of the drawing_canvas.  Used for
        /// determining scroll sizes
        /// </summary>
        public double SizeBuffer
        {
            get
            {
                return 500.0;
            }
        }

        /// <summary>
        /// This is called from main page whenever it gets a key press and drawing drawing_canvas has focus.
        /// </summary>
        public void GotKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && null != selectedElement)
            {
                Core.DrawingCanvasCommands.DeleteSelectedElement(this);
                e.Handled = true;
            }
        }

        /// <summary>
        /// This should only be called whenever the PFD has been changed.  We don't care if the elements moved only if it changes the
        /// PFD
        /// </summary>
        public void PFDModified()
        {
            //get all child PFD elements
            var elements = from c in Children
                           where c is IPfdElement
                           select c as IPfdElement;

            PfdUpdatedEventArgs args = new PfdUpdatedEventArgs(elements);
            PfdUpdated(this, args);
        }

        public void PFDChanging()
        {
            PfdChanging(this, EventArgs.Empty);
        }

        #region Load/Save

        // Keeping commented out for reference purposes, but I think I have all this in the workspace class now

        ///// <summary>
        ///// Loads an XML-generated list of elements.
        ///// </summary>
        ///// <param name="doc"></param>
        //public void LoadXmlElements(XElement doc)
        //{            
        //    // Process units first
        //    XElement processUnits = doc.Descendants("ProcessUnits").ElementAt(0);
        //    foreach (XElement unit in processUnits.Elements())
        //    {
        //        // Create the process unit and add it to the canvas
        //        GenericProcessUnit pu = ChemProV.PFD.ProcessUnits.ProcessUnitFactory.ProcessUnitFromXml(unit);
        //        AddNewChild((UIElement)pu);

        //        // E.O.
        //        // Load any comments that are present
        //        XElement cmtElement = unit.Element("Comments");
        //        if (null != cmtElement)
        //        {
        //            foreach (XElement child in cmtElement.Elements())
        //            {                        
        //                StickyNoteControl sn;
        //                StickyNoteControl.CreateCommentNote(
        //                    this, pu as Core.ICommentCollection, child, out sn);
        //            }
        //        }
        //    }

        //    //then streams
        //    List<ChemProV.PFD.Streams.AbstractStream> streamObjects = new List<ChemProV.PFD.Streams.AbstractStream>();
        //    XElement streamList = doc.Descendants("Streams").ElementAt(0);
        //    foreach (XElement stream in streamList.Elements())
        //    {
        //        // Create the stream. The factory will connect it to the process units and create 
        //        // sticky notes for comments if present
        //        ChemProV.PFD.Streams.AbstractStream s = StreamFactory.StreamFromXml(stream, this, true);

        //        // The stream control itself is really just lines and these lines need a low Z-index
        //        (s as AbstractStream).SetValue(Canvas.ZIndexProperty, -3);

        //        s.UpdateStreamLocation();

        //        //we can't add the streams until we have also built the properties table
        //        //so just add to local list variable
        //        streamObjects.Add(s);
        //    }

        //    //and finally, properties tables
        //    XElement tablesList = doc.Descendants("PropertiesWindows").ElementAt(0);
        //    foreach (XElement table in tablesList.Elements())
        //    {
        //        //store the table's target
        //        string parentName = (string)table.Element("ParentStream");

        //        //create the table
        //        IPropertiesWindow pTable = PropertiesWindowFactory.TableFromXml(table, currentDifficultySetting, isReadOnly);

        //        //find the parent on the drawing_canvas
        //        var parent = from c in streamObjects
        //                     where c.Id.CompareTo(parentName) == 0
        //                     select c;
        //        pTable.ParentStream = parent.ElementAt(0);
        //        parent.ElementAt(0).Table = pTable;

        //        // Add the stream. Streams take care of adding/removing their tables to/from
        //        // the drawing canvas
        //        AddNewChild((UIElement)parent.ElementAt(0));

        //        pTable.TableDataChanged += new TableDataEventHandler(TableDataChanged);
        //        pTable.TableDataChanging += new EventHandler(TableDataChanging);

        //        //tell the stream to redraw in order to fix any graphical glitches
        //        parent.ElementAt(0).UpdateStreamLocation();
        //    }

        //    //don't forget about the sticky notes!
        //    XElement stickyNoteList = doc.Descendants("StickyNotes").ElementAt(0);
        //    foreach (XElement note in stickyNoteList.Elements())
        //    {
        //        StickyNoteControl sn = new StickyNoteControl(note, this);
        //        AddNewChild(sn);
        //    }

        //    // Tell all stream endpoints to update their locations
        //    foreach (UIElement uie in Children)
        //    {
        //        if (!(uie is DraggableStreamEndpoint))
        //        {
        //            continue;
        //        }

        //        DraggableStreamEndpoint dse = uie as DraggableStreamEndpoint;
        //        dse.EndpointConnectionChanged(dse.Type, null, null);
        //    }

        //    // Go through all sticky notes on the canvas and assign colors
        //    Dictionary<string, StickyNoteColors> clrs = new Dictionary<string, StickyNoteColors>();
        //    foreach (UIElement uie in Children)
        //    {
        //        StickyNoteControl sn = uie as StickyNoteControl;
        //        if (null == sn)
        //        {
        //            continue;
        //        }

        //        // Assign a color based on the user name
        //        if (!string.IsNullOrEmpty(sn.CommentUserName))
        //        {
        //            if (!clrs.ContainsKey(sn.CommentUserName))
        //            {
        //                StickyNoteColors clr = StickyNoteControl.GetNextUserStickyColor();
        //                clrs[sn.CommentUserName] = clr;
        //                sn.ColorChange(clr);
        //            }
        //            else
        //            {
        //                sn.ColorChange(clrs[sn.CommentUserName]);
        //            }
        //        }
        //    }

        //    PFDModified();
        //}

        private StickyNoteColors GetStickyNoteColor(StickyNoteControl forThis)
        {
            if (!Core.App.Workspace.UserStickyNoteColors.ContainsKey(forThis.StickyNote.UserName))
            {
                // New user = new color. We need to get a new color and then add this user to
                // the dictionary
                StickyNoteColors clr = StickyNoteControl.GetNextUserStickyColor();
                Core.App.Workspace.UserStickyNoteColors.Add(forThis.StickyNote.UserName, clr);
                return clr;
            }
            else
            {
                return Core.App.Workspace.UserStickyNoteColors[forThis.StickyNote.UserName];
            }
        }

        ///// <summary>
        ///// Turns the drawing drawing_canvas into an XML object
        ///// </summary>
        ///// <param name="writer"></param>
        //public void WriteXml(XmlWriter writer)
        //{
        //    //before writing, separate the elements based on type
        //    List<GenericProcessUnit> processUnits = new List<GenericProcessUnit>();
        //    List<ChemProV.PFD.Streams.AbstractStream> streams = new List<ChemProV.PFD.Streams.AbstractStream>();
        //    List<IPropertiesWindow> PropertiesWindows = new List<IPropertiesWindow>();
        //    List<StickyNoteControl> stickyNotes = new List<StickyNoteControl>();
        //    List<IPfdElement> other = new List<IPfdElement>();

        //    //create the lists by looping through all children
        //    foreach (UIElement element in this.Children)
        //    {
        //        if (element is IPfdElement)
        //        {
        //            if (element is GenericProcessUnit)
        //            {
        //                processUnits.Add(element as GenericProcessUnit);
        //            }
        //            else if (element is ChemProV.PFD.Streams.AbstractStream)
        //            {
        //                streams.Add(element as ChemProV.PFD.Streams.AbstractStream);
        //            }
        //            else if (element is IPropertiesWindow)
        //            {
        //                PropertiesWindows.Add(element as IPropertiesWindow);
        //            }
        //            else if (element is StickyNoteControl)
        //            {
        //                stickyNotes.Add(element as StickyNoteControl);
        //            }
        //            else
        //            {
        //                other.Add(element as IPfdElement);
        //            }
        //        }
        //    }

        //    //process units first
        //    writer.WriteStartElement("ProcessUnits");
        //    foreach (IPfdElement element in processUnits)
        //    {
        //        objectFromIPfdElement(element).Serialize(writer, element);
        //    }
        //    writer.WriteEndElement();

        //    //then streams
        //    writer.WriteStartElement("Streams");
        //    foreach (IPfdElement element in streams)
        //    {
        //        objectFromIPfdElement(element).Serialize(writer, element);
        //    }
        //    writer.WriteEndElement();

        //    //next, properties tables
        //    writer.WriteStartElement("PropertiesWindows");
        //    foreach (IPfdElement element in PropertiesWindows)
        //    {
        //        objectFromIPfdElement(element).Serialize(writer, element);
        //    }
        //    writer.WriteEndElement();

        //    // Write "free-floating" sticky notes. These are ones that have don't have a 
        //    // comment collection parent
        //    writer.WriteStartElement("StickyNotes");
        //    foreach (IPfdElement element in stickyNotes)
        //    {
        //        if (!((StickyNoteControl)element).HasCommentCollectionParent)
        //        {
        //            writer.WriteStartElement("StickyNote");
        //            (element as StickyNoteControl).WriteXml(writer);
        //            writer.WriteEndElement();
        //        }
        //    }
        //    writer.WriteEndElement();
        //}

        /// <summary>
        /// Null class used in XML output.  Does nothing.
        /// </summary>
        public class NullSerializer : IXmlSerializable
        {
            #region IXmlSerializable Members

            public System.Xml.Schema.XmlSchema GetSchema()
            {
                return null;
            }

            public void ReadXml(XmlReader reader)
            {
            }

            public void WriteXml(XmlWriter writer)
            {
            }

            #endregion IXmlSerializable Members
        }

        #endregion Load/Save

        /// <summary>
        /// Resets the drawing drawing_canvas back to its initial state
        /// </summary>
        public void ClearDrawingCanvas()
        {
            this.Children.Clear();
        }

        /// <summary>
        /// I'm creating this method even though children can currently be added via 
        /// Children.Add by any piece of code outside this class. It would be nice 
        /// if we had everything added through this method. Then, if we wanted to do 
        /// some sort of validation in the future we could do it here. Until then, 
        /// it's functionally equivalent to Children.Add
        /// </summary>
        /// <returns>True if the child element was added to the collection of children, 
        /// false otherwise.</returns>
        public bool AddNewChild(UIElement childElement)
        {
            Children.Add(childElement);
            PFDModified();
            return true;
        }

        public bool RemoveChild(UIElement childElement)
        {
            if (!Children.Contains(childElement))
            {
                return false;
            }

            Children.Remove(childElement);
            PFDModified();
            return true;
        }

        public void SetWorkspace(ChemProV.Core.Workspace workspace)
        {
            if (object.ReferenceEquals(m_workspace, workspace))
            {
                // No change
                return;
            }

            // Detach listeners from old workspace
            if (null != m_workspace)
            {
                // This function should really only be called once, so we should never hit this 
                // code, but future versions might change this.
                throw new NotImplementedException();
            }

            // Store a reference to the workspace
            m_workspace = workspace;

            // We must monitor changes to the list of process units, streams, sticky notes, and 
            // comments for streams or process units. (Or should the controls monitor their own 
            // comment collections?)
            m_workspace.ProcessUnitsCollectionChanged += this.ProcessUnits_CollectionChanged;
            m_workspace.StreamsCollectionChanged += new EventHandler(Streams_CollectionChanged);
            m_workspace.StickyNotes.CollectionChanged += this.StickyNotes_CollectionChanged;
            // TODO: The rest
        }

        public Workspace GetWorkspace()
        {
            return m_workspace;
        }

        #region Event handlers for UI updating

        /// <summary>
        /// Callback for when the collection of process units in the workspace changes. We must update 
        /// the UI to match the workspace.
        /// </summary>
        private void ProcessUnits_CollectionChanged(object sender, EventArgs e)
        {
            // First go through all process unit controls on the canvas and remove ones that are 
            // no longer in the workspace.
            List<ChemProV.Core.AbstractProcessUnit> unitsThatHaveControls = 
                new List<ChemProV.Core.AbstractProcessUnit>();
            for (int i=0; i<Children.Count; i++)
            {
                ProcessUnitControl lpu = Children[i] as ProcessUnitControl;
                if (null == lpu)
                {
                    continue;
                }

                // Add it to a list that we'll use later
                unitsThatHaveControls.Add(lpu.ProcessUnit);

                if (!m_workspace.ProcessUnits.Contains(lpu.ProcessUnit))
                {
                    // Tell the process unit to remove itself. It will also remove any 
                    // controls used to represent comments.
                    lpu.RemoveSelfFromCanvas(this);
                    
                    // Reset the index since a bunch of child controls could have potentially just 
                    // been removed
                    i = -1;
                }
            }

            // Now go through and add any process units that are missing
            foreach (ChemProV.Core.AbstractProcessUnit apu in m_workspace.ProcessUnits)
            {
                if (!unitsThatHaveControls.Contains(apu))
                {
                    // Create the process unit. The static method will create it, put it on 
                    // the canvas, and take care of all comment sticky notes as well.
                    ProcessUnitControl lpuNew = ProcessUnitControl.CreateOnCanvas(this, apu);
                }
            }
        }

        /// <summary>
        /// Event handler to update free-floating sticky notes
        /// </summary>
        private void StickyNotes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // First go through all sticky note controls on the canvas and remove ones that are 
            // no longer in the workspace.
            List<ChemProV.PFD.StickyNote.StickyNote_UIIndependent> notesThatHaveControls =
                new List<ChemProV.PFD.StickyNote.StickyNote_UIIndependent>();
            for (int i = 0; i < Children.Count; i++)
            {
                PFD.StickyNote.StickyNoteControl snc = Children[i] as PFD.StickyNote.StickyNoteControl;
                if (null == snc)
                {
                    continue;
                }

                // Add it to a list that we'll use later
                notesThatHaveControls.Add(snc.StickyNote);

                if (!m_workspace.StickyNotes.Contains(snc.StickyNote))
                {
                    // Remove the sticky note control from the canvas
                    Children.RemoveAt(i);

                    // Decrement the index since we just removed a control
                    i = -1;
                }
            }

            // Now go through and add any sticky notes that are missing
            foreach (ChemProV.PFD.StickyNote.StickyNote_UIIndependent sn in m_workspace.StickyNotes)
            {
                if (!notesThatHaveControls.Contains(sn))
                {
                    PFD.StickyNote.StickyNoteControl.CreateOnCanvas(this, sn, null);
                }
            }
        }

        private void Streams_CollectionChanged(object sender, EventArgs e)
        {
            // First go through all stream controls on the canvas and remove ones that are 
            // no longer in the workspace.
            List<ChemProV.Core.AbstractStream> streamsThatHaveControls =
                new List<ChemProV.Core.AbstractStream>();
            for (int i = 0; i < Children.Count; i++)
            {
                PFD.Streams.AbstractStream stream = Children[i] as PFD.Streams.AbstractStream;
                if (null == stream)
                {
                    continue;
                }

                // Add it to a list that we'll use later
                streamsThatHaveControls.Add(stream.Stream);

                if (!m_workspace.Streams.Contains(stream.Stream))
                {
                    // Tell the stream control to remove itself and all it's children (stream lines, 
                    // comment sticky note controls, etc.)
                    stream.RemoveSelfFromCanvas(this);

                    // Reset the index since a bunch of child controls could have potentially just 
                    // been removed
                    i = -1;
                }
            }

            // Now go through and add stream controls that are missing
            foreach (ChemProV.Core.AbstractStream stream in m_workspace.Streams)
            {
                if (!streamsThatHaveControls.Contains(stream))
                {
                    PFD.Streams.AbstractStream.CreateOnCanvas(this, stream);
                }
            }
        }

        #endregion

        public PFD.Streams.AbstractStream GetStreamControl(Core.AbstractStream stream)
        {
            foreach (UIElement uie in Children)
            {
                PFD.Streams.AbstractStream s = uie as PFD.Streams.AbstractStream;
                if (null != s)
                {
                    if (Object.ReferenceEquals(s.Stream, stream))
                    {
                        return s;
                    }
                }
            }

            return null;
        }
    }
}