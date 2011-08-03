/*
Copyright 2010, 2011 HELP Lab @ Washington State University

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

        #region States

        /// <summary>
        /// If it is in resizingState then we are resizing an object
        /// </summary>
        private IState resizingState;

        public IState ResizingState
        {
            get { return resizingState; }
            set { resizingState = value; }
        }

        /// <summary>
        /// If it is in MenuState then the right click menu must be open
        /// </summary>
        private IState menuState;

        public IState MenuState
        {
            get { return menuState; }
            set { menuState = value; }
        }

        /// <summary>
        /// Moving state is when we are moving a pre-existing item.
        /// </summary>
        private IState movingState;

        public IState MovingState
        {
            get { return movingState; }
            set { movingState = value; }
        }

        /// <summary>
        /// This is our default state nothing is happening
        /// </summary>
        private IState nullState;

        public IState NullState
        {
            get { return nullState; }
            set { nullState = value; }
        }

        /// <summary>
        /// This state is the only state that gets set outside of drawingCanvas when the user selects a new item
        /// from the palette on the left the palette throws an interrupt and places drawing drawing_canvas in this state
        /// </summary>
        private IState placingState;

        public IState PlacingState
        {
            get { return placingState; }
            set { placingState = value; }
        }

        private IState selectedState;

        /// <summary>
        /// This occurs when we have an item selected and we are not doing anything else at the moment.
        /// MenuState and MovingState both have priority over selectedState.
        /// </summary>
        public IState SelectedState
        {
            get { return selectedState; }
            set { selectedState = value; }
        }

        /// <summary>
        /// This is a variable that saves the current state.
        /// </summary>
        private IState currentState;

        public IState CurrentState
        {
            get { return currentState; }
            set
            {
                /*
                * we want to clear the redo stack just in case so long as we aren't just going menu state or selecting an object.
                * we also need to tell delete to clear the redoStack because it doesn't happen when we transition to menu but we want it to happen
                * if they delete something.
                */
                if (!(value == menuState || value == nullState || value == selectedState))
                {
                    redoStack.Clear();
                }
                //Ok so going to movingState need to save state before we go there, unless we just came from PlacingState cause we are placing an object no need to save.
                if (value == movingState && currentState != PlacingState)
                {
                    if (selectedElement is StreamSourceIcon)
                    {
                        saveState(CanvasCommands.MoveTail, (selectedElement as StreamEnd).Stream, this, new Point((double)((((selectedElement as StreamEnd).Stream).Source as UIElement).GetValue(Canvas.LeftProperty)), (double)((((selectedElement as StreamEnd).Stream).Source as UIElement).GetValue(Canvas.TopProperty))));
                    }
                    else if (selectedElement is StreamDestinationIcon)
                    {
                        saveState(CanvasCommands.MoveHead, (selectedElement as StreamEnd).Stream, this, new Point((double)((((selectedElement as StreamEnd).Stream).Destination as UIElement).GetValue(Canvas.LeftProperty)), (double)((((selectedElement as StreamEnd).Stream).Destination as UIElement).GetValue(Canvas.TopProperty))));
                    }
                    else if (selectedElement is IProcessUnit)
                    {
                        saveState(CanvasCommands.MoveHead, selectedElement, this, (selectedElement as IProcessUnit).MidPoint);
                    }
                    else if (selectedElement is StickyNote)
                    {
                        saveState(CanvasCommands.MoveHead, selectedElement, this, new Point((double)(selectedElement as UIElement).GetValue(Canvas.LeftProperty), (double)(selectedElement as UIElement).GetValue(Canvas.TopProperty)));
                    }
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
        /// Used to keep a reference to NewContextMenu so it can removed easily
        /// </summary>
        private ContextMenu newContextMenu;

        public ContextMenu NewContextMenu
        {
            get { return newContextMenu; }
            set
            {
                if (newContextMenu != null)
                {
                    //this is so we cannot have multiple right click menus open.
                    this.Children.Remove(newContextMenu);
                }
                newContextMenu = value;
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

        /// <summary>
        /// This stores if the mouse is currently over a IProcessUnit.
        /// </summary>
        private IPfdElement hoveringOver;

        public IPfdElement HoveringOver
        {
            get { return hoveringOver; }
            set { hoveringOver = value; }
        }

        private StickyNote hoveringOverStickyNote;

        public StickyNote HoveringOverStickyNote
        {
            get { return hoveringOverStickyNote; }
            set { hoveringOverStickyNote = value; }
        }

        /// <summary>
        /// This stores the element that is currently selected in the drawing drawing_canvas
        /// </summary>
        private IPfdElement selectedPaletteItem;

        public IPfdElement SelectedPaletteItem
        {
            get
            {
                return selectedPaletteItem;
            }
            set
            {
                //This is how we get to placingState notice it doesn't matter what we were doing if it is changed
                //we go to placingState, error prone?
                selectedPaletteItem = value;
                if (value != null)
                {
                    currentState = placingState;
                }
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
        /// NOTE: the "state" referred to here is not referring to the DrawingCanvas States but closely related.
        /// The undoStack and redoStack.  These save states of the drawing drawing_canvas.  State change happens when something is delete, moved or added.
        /// The undoStack saves previous states right before the transition to a new state.
        /// The RedoStack is added to when an undo is called and it saves the current state before the undo reverts it back.
        /// So the current state is never saved on any stack until it is about to change.
        /// The UndoStack saves any previous states.
        /// And the RedoStack saves any states that happened but were "lost" when the user called undo.
        /// NOTE: Treat like a stack only reason it isn't is if it gets bigger than 25 elements we remove the last one.
        /// </summary>
        public LinkedList<SavedStateObject> undoStack = new LinkedList<SavedStateObject>();

        public LinkedList<SavedStateObject> redoStack = new LinkedList<SavedStateObject>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="command"></param>
        /// <param name="selectedObject"></param>
        /// <param name="canvas"></param>
        /// <param name="lastLocation">This must be the TopLeft of the element if applicable</param>
        public void saveState(CanvasCommands command, IPfdElement selectedObject, Canvas canvas, Point lastLocation)
        {
            //25 is arbitrary we felt that this was a big enough size for the undoStack
            if (undoStack.Count > 25)
            {
                undoStack.RemoveLast();
            }
            if (selectedObject is IStream)
            {
                undoStack.AddFirst(new StreamUndo(command, selectedObject as IStream, canvas, (selectedObject as IStream).Source as IProcessUnit, (selectedObject as IStream).Destination as IProcessUnit, lastLocation));
            }
            else if (selectedObject is IProcessUnit)
            {
                //commented out cause it don't work
                //convert lastLocation to middle because that is what moving expects
                UserControl ui = selectedObject as UserControl;

                //  lastLocation.X = lastLocation.X + ui.Width / 2;
                //  lastLocation.Y = lastLocation.Y + ui.Height / 2;

                undoStack.AddFirst(new ProcessUnitUndo(command, selectedObject as IProcessUnit, canvas, lastLocation));
            }
            else if (selectedObject is StickyNote)
            {
                //convert lastLocation to middle because that is what moving expects
                UserControl ui = selectedObject as UserControl;

                //  lastLocation.X = lastLocation.X + ui.Width / 2;
                //  lastLocation.Y = lastLocation.Y + ui.Height / 2;

                undoStack.AddFirst(new StickyNoteUndo(command, selectedObject as StickyNote, canvas, lastLocation));
            }
        }

        public void Redo()
        {
            //redo doesn't use the parameters
            (menuState as MenuState).Redo(this, new EventArgs());
        }

        public void Undo()
        {
            //undo doesn't use the parameters
            (menuState as MenuState).Undo(this, new EventArgs());
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
            //initialize our states
            menuState = new MenuState(this);
            movingState = new MovingState(this);
            nullState = new NullState(this);
            placingState = new PlacingState(this);
            selectedState = new SelectedState(this);
            resizingState = new ResizeingState(this);

            //set default state
            currentState = nullState;

            //set event listeners
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

        #region DrawingCanvasMouseEvents

        /// <summary>
        /// For all of the possible mouse actions we just call the handler for whatever state we are in.
        /// must set e.handled to true to stop multiple fires if we have layered objects.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseRightButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            currentState.MouseRightButtonUp(sender, e);
            hasFocus = true;
            e.Handled = true;
        }

        public void MouseRightButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            currentState.MouseRightButtonDown(sender, e);
            hasFocus = true;
            if (!isReadOnly)
            {
                e.Handled = true;
            }
        }

        public void MouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            currentState.MouseLeftButtonUp(sender, e);
            hasFocus = true;
            e.Handled = true;
        }

        public void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            currentState.MouseLeftButtonDown(sender, e);
            hasFocus = true;
            e.Handled = true;
        }

        public void MouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
        }

        public void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            currentState.MouseMove(sender, e);
        }

        public void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            currentState.MouseLeave(sender, e);
        }

        public void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            currentState.MouseEnter(sender, e);
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
        /// This is called when we leave an ProcessUnit so we need to the boarder to transparent just in case it got
        /// changed to red or green.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void IProcessUnit_MouseLeave(object sender, MouseEventArgs e)
        {
            //make sure it is not the selectedElement as we don't want to unselect an item just because we moved off it
            if (!sender.Equals(selectedElement))
            {
                (sender as GenericProcessUnit).SetBorderColor(ProcessUnitBorderColor.NoBorder);
            }
            hoveringOver = null;
        }

        /// <summary>
        /// This is called when we move the mouse on top off a ProcessUnit in which case it sets hoveringOver to be itself.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void IProcessUnit_MouseEnter(object sender, MouseEventArgs e)
        {
            hoveringOver = sender as IProcessUnit;
        }

        public void ProcessUnitStreamsChanged(object sender, EventArgs e)
        {
            PFDModified();
        }

        /// <summary>
        /// This is called whenever a temporary process unit is clicked on.  Checks to see if it is a source
        /// or destination by checking if it has incoming or outgoing streams then call currentState.MouseLeftButtonDown
        /// on the class either streamDest or Stream Source this is need because it holds the stream and which end
        /// we need to move
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TempProcessUnitMouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if ((sender as IProcessUnit).IncomingStreams.Count > 0)
            {
                currentState.MouseLeftButtonDown((((sender as IProcessUnit).IncomingStreams[0] as IStream).StreamDestination), e);
            }
            if ((sender as IProcessUnit).OutgoingStreams.Count > 0)
            {
                currentState.MouseLeftButtonDown((((sender as IProcessUnit).OutgoingStreams[0] as IStream).StreamSource), e);
            }

            e.Handled = true;
        }

        /// <summary>
        /// This is fired when the arrow is clicked on and it mimics what happens when u click on the sink
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TailMouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            currentState.MouseLeftButtonDown((sender as IStream).StreamSource, e);
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
            //first we pull all the IPfdElement from the drawing canvas then we check to see if we can do the conversion
            //if not throw an exception if we can then we need to check to see if we need to change our IPropertiesWindow
            //and if we do then change them

            List<IPfdElement> pfdElements = this.ChildIPfdElements;

            if (newValue < currentDifficultySetting && newValue == OptionDifficultySetting.MaterialBalanceWithReactors)
            {
                //this means we could have heat streams or heat exchanges that we need to check for
                foreach (IPfdElement element in pfdElements)
                {
                    if (element is HeatStream)
                    {
                        throw new Exception("Heat streams where detected but this file says it is supposed to be only MaterialBalanceWithReactors");
                    }
                    else if (element is IProcessUnit)
                    {
                        if ((element as IProcessUnit).Description == ProcessUnitDescriptions.HeatExchanger ||
                            (element as IProcessUnit).Description == ProcessUnitDescriptions.HeatExchangerNoUtility)
                        {
                            throw new Exception("Advanced process units where detected but this file says it is supposed to be only MaterialBalanceWithReactors");
                        }
                    }
                }
            }

            else if (newValue < currentDifficultySetting && newValue == OptionDifficultySetting.MaterialBalance)
            {
                //this means we could have heat streams, reactors, or heat exchanges that we need to check for
                foreach (IPfdElement element in pfdElements)
                {
                    if (element is HeatStream)
                    {
                        throw new Exception("Heat streams where detected but this file says it is supposed to be only MaterialBalance");
                    }
                    else if (element is IProcessUnit)
                    {
                        if ((element as IProcessUnit).Description == ProcessUnitDescriptions.HeatExchanger ||
                            (element as IProcessUnit).Description == ProcessUnitDescriptions.HeatExchangerNoUtility ||
                            (element as IProcessUnit).Description == ProcessUnitDescriptions.Reactor)
                        {
                            throw new Exception("Advanced process units where detected but this file says it is supposed to be only MaterialBalance");
                        }
                    }
                }
            }

            if (newValue == OptionDifficultySetting.MaterialAndEnergyBalance || oldValue == OptionDifficultySetting.MaterialAndEnergyBalance)
            {
                //got to add or remove temp from the tables it will figured out what one to do based on the newValue
                for (int i = 0; i < pfdElements.Count; i++)
                {
                    IPfdElement element = pfdElements[i];
                    if (element is IPropertiesWindow)
                    {
                        CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, element, this).Execute();
                        element = PropertiesWindowFactory.TableFromTable((element as IPropertiesWindow), newValue, isReadOnly);
                        CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, element, this).Execute();
                    }
                }
            }
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
            if (e.Key == Key.Delete && currentState == selectedState)
            {
                if (selectedElement is ChemicalStreamPropertiesWindow)
                {
                    // (selectedElement as ChemicalStreamPropertiesWindow).PropertiesWindow_KeyDown(sender, e);
                }
                else
                {
                    //these arguments are not used so it doesn't care what they are
                    (menuState as MenuState).Delete(sender, e);
                }
            }
            else if (e.Key == Key.Z && (Keyboard.Modifiers == ModifierKeys.Control))
            {
                //these arguments are not used so it doesn't care what they are
                (menuState as MenuState).Undo(this, EventArgs.Empty);
            }
            else if (e.Key == Key.Y && (Keyboard.Modifiers == ModifierKeys.Control))
            {
                //these arguments are not used so it doesn't care what they are
                (menuState as MenuState).Redo(this, EventArgs.Empty);
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
            //Process units first
            XElement processUnits = doc.Descendants("ProcessUnits").ElementAt(0);
            foreach (XElement unit in processUnits.Elements())
            {
                //create the process unit
                IProcessUnit pu = ProcessUnitFactory.ProcessUnitFromXml(unit);

                //create & execute the command to place the process unit onto the drawing_canvas
                Commands.ICommand cmd = CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, pu, this, new Point(-1.0, -1.0));
                cmd.Execute();
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

                //add the stream, and therefore the table to the drawing_canvas
                Commands.ICommand cmd = CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, parent.ElementAt(0), this, new Point(-1.0, -1.0));
                cmd.Execute();

                //tell the stream to redraw in order to fix any graphical glitches
                parent.ElementAt(0).UpdateStreamLocation();
            }

            //don't forget about the sticky notes!
            XElement stickyNoteList = doc.Descendants("StickyNotes").ElementAt(0);
            foreach (XElement note in stickyNoteList.Elements())
            {
                StickyNote sn = StickyNote.FromXml(note);

                //create & execute the command to place the process unit onto the drawing_canvas
                Commands.ICommand cmd = CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, sn, this, new Point(-1.0, -1.0));
                cmd.Execute();
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
            currentState = resizingState;
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

        public void StickyNote_Closing(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement parent = sender as FrameworkElement;

            while (!(parent is IPfdElement))
            {
                parent = parent.Parent as FrameworkElement;
            }

            selectedElement = parent as IPfdElement;

            //call the Delete in menu state so it can remove the stickyNote
            (menuState as MenuState).Delete(selectedElement, EventArgs.Empty);
        }

        public void newStickyNote_MouseEnter(object sender, MouseEventArgs e)
        {
            HoveringOverStickyNote = sender as StickyNote;
        }

        public void newStickyNote_MouseLeave(object sender, MouseEventArgs e)
        {
            HoveringOverStickyNote = null;
        }

        #endregion StickyNotes
    }
}