/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using ChemProV.PFD;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.StickyNote;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.PFD.Streams.PropertiesWindow.Chemical;
using ChemProV.PFD.Streams.PropertiesWindow.Heat;
using ChemProV.UI.DrawingCanvas.Commands;
using ChemProV.UI.DrawingCanvas.States;

namespace ChemProV.UI.DrawingCanvas
{
    public delegate void PfdUpdatedEventHandler(object sender, PfdUpdatedEventArgs e);

    /// <summary>
    /// This is our drawing drawing_canvas, the thing that ProcessUnits and Streams are dragged onto.
    /// It controls everything that goes on with itself.
    /// </summary>
    public class DrawingCanvas : Canvas, IXmlSerializable
    {
        public event PfdUpdatedEventHandler PfdUpdated = delegate { };
        public event EventHandler PfdChanging = delegate { };

        /// <summary>
        /// This is a debugging-oriented state tracker. When the CurrentState property is set to 
        /// some value, we check to see if our current state is non-null. If it is non-null then 
        /// the contract is that we let that state know that it's ending and we're going to a new 
        /// one. We do this by calling the stat's StateEnding() function. Before we call we set 
        /// this to true and after the call returns we set it back to false.
        /// This allows us to ensure that the StateEnding() function doesn't try to set 
        /// CurrentState again. States are not allowed to set the drawing canvas's state from 
        /// their StateEnding() functions.
        /// In theory this could be enforced using reflection and eliminate the need for this 
        /// variable, but that's something that I'll look into later.
        /// </summary>
        private bool m_endingAState = false;

        #region States

        /// <summary>
        /// This is a variable that saves the current state.
        /// </summary>
        private IState currentState = null;

        public IState CurrentState
        {
            get { return currentState; }
            set
            {
                // Start with a check to see if we're currently ending a state
                if (m_endingAState)
                {
                    throw new InvalidOperationException(
                        "Objects that implement IState are NOT permitted to set the " +
                        "DrawingCanvas.CurrentState property when in their StateEnding() method.");
                }
                
                // The contract for items that implement IState is that when we're switching from 
                // them to something else, we let them know that their state is ending
                if (null != currentState &&
                    !object.ReferenceEquals(value, currentState))
                {
                    m_endingAState = true;
                    currentState.StateEnding();
                    m_endingAState = false;
                }

                currentState = value;
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

        private OptionDifficultySetting currentDifficultySetting;

        /// <summary>
        /// get / set the currentlyDifficult.  Set will throw an exception if it cannot handle the request and
        /// it will keep its old value
        /// </summary>
        public OptionDifficultySetting CurrentDifficultySetting
        {
            get { return currentDifficultySetting; }
            set
            {
                DifficultySettingChanged(currentDifficultySetting, value);
                currentDifficultySetting = value;
            }
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

        //if value = moving state got to save 'previous location' which would be current location at that time.
        //if currentState is PlacingState need to set PreviousLocation to null so index know index need to delete if index undo

        /// <summary>
        /// This stores the currently selected element, the one with the yellow boarder
        /// </summary>
        private IPfdElement selectedElement;

        public IPfdElement SelectedElement
        {
            get { return selectedElement; }
            set
            {
                //This checks to see if we had an element selected if so tell it it is no longer selected
                IPfdElement oldvalue = selectedElement;
                if (oldvalue != null)
                {
                    oldvalue.Selected = false;
                }

                //If we are selecting something tell it to be selected all PFDElements have an event when Selected
                //is changed and draw the boarder around themselves appropriately.
                if (value != null)
                {
                    value.Selected = true;
                    if (value is UserControl)
                    {
                        (value as UserControl).Focus();
                    }
                }

                selectedElement = value;
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

        #region Undo/Redo

        /// <summary>
        /// E.O.
        /// This is the undo stack. When the "Undo()" function is called, the top collection will be 
        /// popped of this stack and executed. The return value from the execution function will be 
        /// pushed onto the redo stack.
        /// </summary>
        private Stack<UndoRedoCollection> m_undos = new Stack<UndoRedoCollection>();

        /// <summary>
        /// E.O.
        /// This is the redo stack. When the "Redo()" function is called, the top collection will be 
        /// popped of this stack and executed. The return value from the execution function will be 
        /// pushed onto the undo stack.
        /// NEVER add anything to this stack or m_undos. Use the AddUndo function. The undo system 
        /// is intentially designed so that redos are created automatically in the Undo() function. 
        /// The Undo() function should be the ONLY place where you see m_redos.Push and the Redo() 
        /// function is the ONLY place where you should see m_redos.Pop();
        /// </summary>
        private Stack<UndoRedoCollection> m_redos = new Stack<UndoRedoCollection>();

        /// <summary>
        /// E.O.
        /// Adds an undo action to the undo stack. You'll notice there is no AddRedo function. This is 
        /// intentional because upon execution of an undo (via a call to "Undo()") the redo action is 
        /// automatically created and pushed onto the redo stack.
        /// 
        /// Will remove exception throw when I get the new undo system in place
        /// </summary>
        /// <param name="collection">Collection of undo actions to push.</param>
        /// <returns>True if the collection was successfully added to the stack, false otherwise.</returns>
        public bool AddUndo(UndoRedoCollection collection)
        {
            // E.O.
            m_undos.Push(collection);

            // Adding a new undo clears the redo stack
            m_redos.Clear();

            return true;
        }

        /// <summary>
        /// E.O.
        /// Gets the number of undos currently on the undo stack.
        /// </summary>
        public int UndoCount
        {
            get
            {
                return m_undos.Count;
            }
        }

        public string UndoTitle
        {
            get
            {
                if (0 == m_undos.Count)
                {
                    return "Undo";
                }
                return m_undos.Peek().Title;
            }
        }

        /// <summary>
        /// E.O.
        /// Gets the number of redos currently on the redo stack.
        /// </summary>
        public int RedoCount
        {
            get
            {
                return m_redos.Count;
            }
        }

        public string RedoTitle
        {
            get
            {
                if (0 == m_redos.Count)
                {
                    return "Redo";
                }
                return m_redos.Peek().Title;
            }
        }

        public void Redo()
        {
            // Set the state to null just in case (risky?)
            CurrentState = null;

            // E.O.
            if (m_redos.Count > 0)
            {
                // Logic:
                // 1. Pop redo collection on top of stack
                // 2. Execute it
                // 3. Take its return value and push it onto the undo stack
                // (done in 1 line below)
                m_undos.Push(m_redos.Pop().Execute(new UndoRedoExecutionParameters(this)));
            }
        }

        public void Undo()
        {
            // First we flip back to the null state
            CurrentState = null;

            // Then we execute the undo (if there is one)
            if (m_undos.Count > 0)
            {
                // Logic:
                // 1. Pop undo collection on top of stack
                // 2. Execute it
                // 3. Take its return value and push it onto the redo stack
                // (done in 1 line below)
                m_redos.Push(m_undos.Pop().Execute(new UndoRedoExecutionParameters(this)));
            }
        }

        #endregion Undo/Redo

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
        /// This is called whenever we have placed a new tool and it fires an event in pallet so it sets the
        /// selection back to the arrow
        /// </summary>
        public void placedNewTool()
        {
            this.ToolPlaced(this, new EventArgs());
        }

        /// <summary>
        /// E.O.
        /// I'm a little surprised this functionality doesn't already exist in the Silverlight 
        /// Canvas control, but whatever.
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
                if (object.ReferenceEquals(excludeMe, uie))
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
            SelectedElement = GetChildAt(e.GetPosition(this), null) as IPfdElement;

            // A right mouse button down implies that we need to flip to the menu state
            CurrentState = new UI.DrawingCanvas.States.MenuState(this, e.GetPosition(this));
        }

        public void MouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (null != currentState)
            {
                currentState.MouseLeftButtonUp(sender, e);
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
            if (null != currentState)
            {
                currentState.MouseLeftButtonDown(sender, e);
                return;
            }

            // If our current state is null, then we want to create an appropriate one.
            // But first we need to check to see if we've selected an element
            object childAtPos = GetChildAt(e.GetPosition(this), null);
            SelectedElement = childAtPos as IPfdElement;

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
                currentState = selectedObjState;
            }
            else
            {
                // If the selected element does not have its own mouse processing logic then 
                // we create a MovingState object, which will drag around the selected element.
                currentState = MovingState.Create(this);
            }

            // Finish up by sending this mouse event to the current state
            if (null != currentState)
            {
                currentState.MouseLeftButtonDown(sender, e);
            }
        }

        public void MouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
        }

        public void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (null != currentState)
            {
                currentState.MouseMove(sender, e);
            }
        }

        public void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            if (null != currentState)
            {
                currentState.MouseLeave(sender, e);
            }
        }

        public void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            if (null != currentState)
            {
                currentState.MouseEnter(sender, e);
            }
        }

        #endregion DrawingCanvasMouseEvents

        #region IPfdMouseEvents

        public void TempProcessUnitMouseRightButtonDownHandler(object sender, MouseEventArgs e)
        {
            TemporaryProcessUnit tpu = sender as TemporaryProcessUnit;
            if (tpu.OutgoingStreams.Count > 0)
            {
                SelectedElement = tpu.OutgoingStreams[0];
            }
            else if (tpu.IncomingStreams.Count > 0)
            {
                SelectedElement = tpu.IncomingStreams[0];
            }
        }

        /// <summary>
        /// This is called when we move the mouse on top off a ProcessUnit in which case it sets hoveringOver to be itself.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void IProcessUnit_MouseEnter(object sender, MouseEventArgs e)
        {
        }

        public void ProcessUnitStreamsChanged(object sender, EventArgs e)
        {
            PFDModified();
        }

        /// <summary>
        /// This is fired when the rectangle at the start of the stream is clicked on and it mimics what happens when u click on the source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HeadMouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            currentState.MouseLeftButtonDown((sender as IStream).StreamDestination, e);
        }

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

        public void DifficultySettingChanged(OptionDifficultySetting oldValue, OptionDifficultySetting newValue)
        {
            // E.O.
            // I've commented out what was here previously and placed it below my new implementation. The first two 
            // checks seemed straightforward enough, but the last check that is checking to see if the old and new 
            // values are the same and are both MaterialAndEnergyBalance is just confusing. I'd be surprised if it's 
            // not a logic bug (that I'm fixing by adding my changes) but just in case there's something I missed, I'm 
            // leaving it around.

            // Go through all the child elements looking for streams and process units. Upon finding either, ask it 
            // if it's available at the difficulty level that we want to switch to and if it's not, throw an exception.
            foreach (UIElement uie in Children)
            {
                if (uie is IStream)
                {
                    System.Reflection.MethodInfo mi = uie.GetType().GetMethod("IsAvailableWithDifficulty");
                    bool available = (bool)mi.Invoke(null, new object[] { newValue });
                    if (!available)
                    {
                        // This exception is caught at a higher level and an appropriate error message is shown
                        throw new Exception();
                    }
                }
                else if (uie is IProcessUnit)
                {
                    if (!(uie as IProcessUnit).IsAvailableWithDifficulty(newValue))
                    {
                        // This exception is caught at a higher level and an appropriate error message is shown
                        throw new Exception();
                    }
                }
            }

            ////first we pull all the IPfdElement from the drawing canvas then we check to see if we can do the conversion
            ////if not throw an exception if we can then we need to check to see if we need to change our IPropertiesWindow
            ////and if we do then change them

            //List<IPfdElement> pfdElements = this.ChildIPfdElements;

            //if (newValue < currentDifficultySetting && newValue == OptionDifficultySetting.MaterialBalanceWithReactors)
            //{
            //    //this means we could have heat streams or heat exchanges that we need to check for
            //    foreach (IPfdElement element in pfdElements)
            //    {
            //        if (element is HeatStream)
            //        {
            //            throw new Exception("Heat streams where detected but this file says it is supposed to be only MaterialBalanceWithReactors");
            //        }
            //        else if (element is IProcessUnit)
            //        {
            //            if ((element as IProcessUnit).Description == ProcessUnitDescriptions.HeatExchanger ||
            //                (element as IProcessUnit).Description == ProcessUnitDescriptions.HeatExchangerNoUtility)
            //            {
            //                throw new Exception("Advanced process units where detected but this file says it is supposed to be only MaterialBalanceWithReactors");
            //            }
            //        }
            //    }
            //}

            //else if (newValue < currentDifficultySetting && newValue == OptionDifficultySetting.MaterialBalance)
            //{
            //    //this means we could have heat streams, reactors, or heat exchanges that we need to check for
            //    foreach (IPfdElement element in pfdElements)
            //    {
            //        if (element is HeatStream)
            //        {
            //            throw new Exception("Heat streams where detected but this file says it is supposed to be only MaterialBalance");
            //        }
            //        else if (element is IProcessUnit)
            //        {
            //            if ((element as IProcessUnit).Description == ProcessUnitDescriptions.HeatExchanger ||
            //                (element as IProcessUnit).Description == ProcessUnitDescriptions.HeatExchangerNoUtility ||
            //                (element as IProcessUnit).Description == ProcessUnitDescriptions.Reactor)
            //            {
            //                throw new Exception("Advanced process units where detected but this file says it is supposed to be only MaterialBalance");
            //            }
            //        }
            //    }
            //}

            //if (newValue == OptionDifficultySetting.MaterialAndEnergyBalance || oldValue == OptionDifficultySetting.MaterialAndEnergyBalance)
            //{
            //    //got to add or remove temp from the tables it will figured out what one to do based on the newValue
            //    for (int i = 0; i < pfdElements.Count; i++)
            //    {
            //        IPfdElement element = pfdElements[i];
            //        if (element is IPropertiesWindow)
            //        {
            //            CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, element, this).Execute();
            //            element = PropertiesWindowFactory.TableFromTable((element as IPropertiesWindow), newValue, isReadOnly);
            //            CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, element, this).Execute();
            //        }
            //    }
            //}
        }

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
        /// NOTE: HasFocus1 is what we use for focus since drawing_canvas cannot have built-in focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void GotKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && null != selectedElement)
            {
                Core.DrawingCanvasCommands.DeleteSelectedElement(this);
            }
            else if (e.Key == Key.Z && (Keyboard.Modifiers == ModifierKeys.Control))
            {
                Undo();
            }
            else if (e.Key == Key.Y && (Keyboard.Modifiers == ModifierKeys.Control))
            {
                Redo();
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

        /// <summary>
        /// Loads an XML-generated list of elements.
        /// </summary>
        /// <param name="doc"></param>
        public void LoadXmlElements(XElement doc)
        {
            // Process units first
            XElement processUnits = doc.Descendants("ProcessUnits").ElementAt(0);
            foreach (XElement unit in processUnits.Elements())
            {
                // Create the process unit and add it to the canvas
                IProcessUnit pu = ProcessUnitFactory.ProcessUnitFromXml(unit);
                AddNewChild((UIElement)pu);
            }

            //then streams
            List<IStream> streamObjects = new List<IStream>();
            XElement streamList = doc.Descendants("Streams").ElementAt(0);
            foreach (XElement stream in streamList.Elements())
            {
                //create the stream
                IStream s = StreamFactory.StreamFromXml(stream);

                //set the source and destination
                var targetNames = from c in stream.DescendantsAndSelf()
                                  select new
                                  {
                                      Source = (string)c.Element("Source"),
                                      Destination = (string)c.Element("Destination")
                                  };

                //find the source in the current list of children
                var source = from c in Children
                             where c is IProcessUnit
                             &&
                             ((c as IPfdElement).Id.CompareTo(targetNames.ElementAt(0).Source) == 0)
                             select c;

                //and the destination
                var dest = from c in Children
                           where c is IProcessUnit
                           &&
                           ((c as IPfdElement).Id.CompareTo(targetNames.ElementAt(0).Destination) == 0)
                           select c;

                //set the source and destination of the stream
                s.Source = source.ElementAt(0) as IProcessUnit;
                s.Destination = dest.ElementAt(0) as IProcessUnit;

                //we can't add the streams until we have also built the properties table
                //so just add to local list variable
                streamObjects.Add(s);
            }

            //and finally, properties tables
            XElement tablesList = doc.Descendants("PropertiesWindows").ElementAt(0);
            foreach (XElement table in tablesList.Elements())
            {
                //store the table's target
                string parentName = (string)table.Element("ParentStream");

                //create the table
                IPropertiesWindow pTable = PropertiesWindowFactory.TableFromXml(table, currentDifficultySetting, isReadOnly);

                //find the parent on the drawing_canvas
                var parent = from c in streamObjects
                             where c.Id.CompareTo(parentName) == 0
                             select c;
                pTable.ParentStream = parent.ElementAt(0);
                parent.ElementAt(0).Table = pTable;

                // E.O.
                // Add the stream, and therefore the table to the drawing_canvas
                AddNewChild((UIElement)parent.ElementAt(0));
                // Below was what was here originally. I'm still unsure about some of the stream-related 
                // initialization stuff
                //Commands.ICommand cmd = CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, parent.ElementAt(0), this, new Point(-1.0, -1.0));
                //cmd.Execute();

                //tell the stream to redraw in order to fix any graphical glitches
                parent.ElementAt(0).UpdateStreamLocation();
            }

            //don't forget about the sticky notes!
            XElement stickyNoteList = doc.Descendants("StickyNotes").ElementAt(0);
            foreach (XElement note in stickyNoteList.Elements())
            {
                StickyNote sn = StickyNote.FromXml(note);
                AddNewChild(sn);
            }

            //kind of a hack, but the rule checker fails during object creation for obvious reasons.
            //In order to get around this, we're delaying the attaching of streams to process units,
            //which essentially prevents the error checker from working.  We do this at the end after
            //everything has been created an added to the drawing_canvas
            foreach (IStream s in streamObjects)
            {
                s.Source.AttachOutgoingStream(s);
                s.Destination.AttachIncomingStream(s);
                s.UpdateStreamLocation();
            }
        }

        #region IXmlSerializable Members

        /// <summary>
        /// According to the MSDN documentation, this should return NULL
        /// </summary>
        /// <returns></returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// not used
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(XmlReader reader)
        {
        }

        /// <summary>
        /// Turns the drawing drawing_canvas into an XML object
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer)
        {
            //before writing, separate the elements based on type
            List<IProcessUnit> processUnits = new List<IProcessUnit>();
            List<IStream> streams = new List<IStream>();
            List<IPropertiesWindow> PropertiesWindows = new List<IPropertiesWindow>();
            List<StickyNote> stickyNotes = new List<StickyNote>();
            List<IPfdElement> other = new List<IPfdElement>();

            //create the lists by looping through all children
            foreach (UIElement element in this.Children)
            {
                if (element is IPfdElement)
                {
                    if (element is IProcessUnit)
                    {
                        processUnits.Add(element as IProcessUnit);
                    }
                    else if (element is IStream)
                    {
                        streams.Add(element as IStream);
                    }
                    else if (element is IPropertiesWindow)
                    {
                        PropertiesWindows.Add(element as IPropertiesWindow);
                    }
                    else if (element is StickyNote)
                    {
                        stickyNotes.Add(element as StickyNote);
                    }
                    else
                    {
                        other.Add(element as IPfdElement);
                    }
                }
            }

            //process units first
            writer.WriteStartElement("ProcessUnits");
            foreach (IPfdElement element in processUnits)
            {
                objectFromIPfdElement(element).Serialize(writer, element);
            }
            writer.WriteEndElement();

            //then streams
            writer.WriteStartElement("Streams");
            foreach (IPfdElement element in streams)
            {
                objectFromIPfdElement(element).Serialize(writer, element);
            }
            writer.WriteEndElement();

            //next, properties tables
            writer.WriteStartElement("PropertiesWindows");
            foreach (IPfdElement element in PropertiesWindows)
            {
                objectFromIPfdElement(element).Serialize(writer, element);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("StickyNotes");
            foreach (IPfdElement element in stickyNotes)
            {
                objectFromIPfdElement(element).Serialize(writer, element);
            }
            writer.WriteEndElement();

            //finally, whatever is left over
            writer.WriteStartElement("Other");
            foreach (IPfdElement element in other)
            {
                objectFromIPfdElement(element).Serialize(writer, element);
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// Helper function used to get the right type of XML Serialize
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private XmlSerializer objectFromIPfdElement(IPfdElement element)
        {
            if (element is GenericProcessUnit)
            {
                return new XmlSerializer(typeof(GenericProcessUnit));
            }
            else if (element is AbstractStream)
            {
                return new XmlSerializer(typeof(AbstractStream));
            }
            else if (element is ChemicalStreamPropertiesWindow)
            {
                return new XmlSerializer(typeof(ChemicalStreamPropertiesWindow));
            }
            else if (element is HeatStreamPropertiesWindow)
            {
                return new XmlSerializer(typeof(HeatStreamPropertiesWindow));
            }
            else if (element is StickyNote)
            {
                return new XmlSerializer(typeof(StickyNote));
            }
            return new XmlSerializer(typeof(NullSerializer));
        }

        #endregion IXmlSerializable Members

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
            ChemicalStreamPropertiesWindow.ResetTableCounter();
            HeatStreamPropertiesWindow.ResetTableCounter();
        }

        #region StickyNotes

        public void newNote_Resizing(object sender, EventArgs e)
        {
            this.Cursor = Cursors.SizeNWSE;
            selectedElement = sender as IPfdElement;
            (sender as StickyNote).CaptureMouse();
            
            //currentState = resizingState;
            // TODO
            throw new InvalidOperationException("This code needs to be fixed or removed");
        }

        /// <summary>
        /// This is called by the rectangle in the header of a StickyNote so we need to get a pointer to the StickyNote itself.
        /// We do this by calling parent until we get an IPfdElement which must be our stickyNote.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StickyNoteMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement parent = sender as FrameworkElement;

            //this tell the label to grab the mouse
            parent.CaptureMouse();

            while (!(parent is IPfdElement))
            {
                parent = parent.Parent as FrameworkElement;
            }

            currentState.MouseLeftButtonDown(parent, e);
        }

        #endregion StickyNotes

        /// <summary>
        /// I'm creating this method even though children can currently be added via 
        /// Children.Add by any piece of code outside this class. It would be nice 
        /// if we had everything add through this method. Then, if we wanted to do 
        /// some sort of validation in the future we could do it here. Until then, 
        /// it's functionally equivalent to Children.Add
        /// </summary>
        /// <returns>True if the child element was added to the collection of children, 
        /// false otherwise.</returns>
        public bool AddNewChild(UIElement childElement)
        {
            Children.Add(childElement);
            return true;
        }

        public bool RemoveChild(UIElement childElement)
        {
            if (!Children.Contains(childElement))
            {
                return false;
            }

            Children.Remove(childElement);
            return true;
        }
    }
}