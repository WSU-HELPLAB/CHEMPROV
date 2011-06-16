using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using ChemProV.PFD;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Streams.PropertiesTable;
using ChemProV.PFD.Streams.PropertiesTable.Chemical;
using ChemProV.UI.DrawingCanvas.Commands;
using ChemProV.UI.DrawingCanvas.Commands.ProcessUnit;
using ChemProV.UI.DrawingCanvas.Commands.Stream;
using ChemProV.UI.DrawingCanvas.States;
using ChemProV.Validation.Rules;
using ChemProV.Validation;

namespace ChemProV.UI.DrawingCanvas
{
    public delegate void PfdUpdatedEventHandler(object sender, PfdUpdatedEventArgs e);

    /// <summary>
    /// This is our drawing canvas, the thing that ProcessUnits and Streams are dragged onto.
    /// It controls everything that goes on with itself.
    /// </summary>
    public class DrawingCanvas : Canvas, IXmlSerializable
    {
        
        public event PfdUpdatedEventHandler PfdUpdated = delegate { };

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
        /// NOTE: "the state" referred to here is not the same as the DrawingCanvas but closely related.
        /// The undoStack and redoStack.  These save states of the drawing canvas.  State change happens when something is delete, moved or added.
        /// The undoStack saves previous states right before the transition to a new state.
        /// The redostack is added to when an undo is called and it saves the current state before the undo reverts it back.
        /// So the current state is never saved on any stack until it is about to change.
        /// The undostack saves any previous states.
        /// And the redostack saves any states that happened but were "lost" when the user called undo.
        /// NOTE: Treat like a stack only reason it isn't is if it gets bigger than 25 elements we remove the last one.
        /// </summary>
        public LinkedList<SavedStateObject> undoStack = new LinkedList<SavedStateObject>();

        public LinkedList<SavedStateObject> redoStack = new LinkedList<SavedStateObject>();

        /// <summary>
        /// Used to keep a reference to rightClickMenu so it can removed easily
        /// </summary>
        public ContextMenu rightClickMenu;

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
        /// from the palette on the left the palette throws an interrupt and places drawing canvas in this state
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
        /// MenuState and Movingstate both have priority over selectedState.
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
                * we want to clear the redostack just incase so long as we aren't just going menu state or selecting an object.
                * we also need to tell delete to clear teh redoStack because it doesn't happen when we transition to menu but we want it to happen
                * if they delete something.
                */
                if (!(value == menuState || value == nullState || value == selectedState))
                {
                    redoStack.Clear();
                }
                //Ok so going to movingState need to save state before we go there, unless we just came from placingstate cause we are placing an object no need to save.
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
                    else if(selectedElement is IProcessUnit)
                    {
                        saveState(CanvasCommands.MoveHead, selectedElement, this, new Point((double)(selectedElement as UIElement).GetValue(Canvas.LeftProperty), (double)(selectedElement as UIElement).GetValue(Canvas.TopProperty)));
                    }
                }
                currentState = value;
            }
        }

        //if value = moving state gotta save 'previouslocation' which would be current location at that time.
        //if currentState is placingstate need to set previouslocation to null so i know i need to delete if i undo

        /// <summary>
        /// This stores the currently selected element, the one with the yellow boarder
        /// </summary>
        private IPfdElement selectedElement;

        public IPfdElement SelectedElement
        {
            get { return selectedElement; }
            set
            {
                /////////////////////new code for rule checker/////////////////////////////////////////////
                //if (value is IProcessUnit)
                //{
                //    IRule someRule = RuleFactory.RuleFromPfdElement(value);
                //    someRule.CheckRule();
                //    MessageBox.Show("IS VALID?  " + someRule.IsValid);
                //}
                ////////////////////////end of new code for rule checker////////////////////////////////

                //This checks to see if we had an elment selected if so tell it it is no longer selected
                IPfdElement oldvalue = selectedElement;
                if (oldvalue != null)
                {
                    oldvalue.Selected = false;
                }

                //If we are selecting something tell it to be selected all PFDElements have an event when Selected
                //is changed and draw the boarder around themselves appropiately.
                if (value != null)
                {
                    value.Selected = true;
                }
                selectedElement = value;
            }
        }

        /// <summary>
        /// This stores if the mouse is currently over a variable.
        /// </summary>
        private IPfdElement hoveringOver;

        public IPfdElement HoveringOver
        {
            get { return hoveringOver; }
            set { hoveringOver = value; }
        }

        /// <summary>
        /// This stores the element that is currently selected in the drawing canvas
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

        /// <summary>
        /// Raised whenever the canvas places a drawing tool
        /// </summary>
        public event EventHandler ToolPlaced = delegate { };
        public event EventHandler feedbackLabelEvent = delegate { };

        /// <summary>
        /// This is the contructor, we make the states we will be using and set the mouse events.
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
        /// Resets the clipping region of the drawing canvas whenever it gets resized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
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
        /// For all of the possible mouse actions we just call the handler for whatever state we are in.
        /// must set e.handled to true to stop multipul fires if we have layered objects.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseRightButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            currentState.MouseRightButtonUp(sender, e);
            e.Handled = true;
        }

        public void MouseRightButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            currentState.MouseRightButtonDown(sender, e);
            e.Handled = true;
        }

        public void MouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            currentState.MouseLeftButtonUp(sender, e);
            e.Handled = true;
        }

        public void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            currentState.MouseLeftButtonDown(sender, e);
            e.Handled = true;
        }

        public void MouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            currentState.MouseWheel(sender, e);
            e.Handled = true;
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

        /// <summary>
        /// This is called when we leave an ProcessUnit so we need to the boarder to transparent just incase it got
        /// changed to red or green.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void IProcessUnit_MouseLeave(object sender, MouseEventArgs e)
        {
            //make sure it is not the selectedElement as we dont want to unselect an item just because we moved off it
            if (!sender.Equals(selectedElement))
            {
                (sender as GenericProcessUnit).SetBorderColor(new SolidColorBrush(Colors.Transparent));
            }
            hoveringOver = null;
        }

        /// <summary>
        /// This is called when we move the mouse ontop off a ProcessUnit in which case it sets hoveringOver to be itself.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void IProcessUnit_MouseEnter(object sender, MouseEventArgs e)
        {
            hoveringOver = sender as IProcessUnit;
        }

        /// <summary>
        /// This is called whenever a temporary prcoess unit is clicked on.  Checks to see if it is a source 
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
        /// This is fired when the arrow is clicked on and it miniks what happens when u click on the sink
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TailMouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            currentState.MouseLeftButtonDown((sender as IStream).StreamSource, e);
        }

        /// <summary>
        /// This is fired when the rectangle at the start of the stream is clicked on and it miniks what happens when u click on the source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HeadMouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            currentState.MouseLeftButtonDown((sender as IStream).StreamDestination, e);
        }

        public void saveState(CanvasCommands command, IPfdElement selectedObject, Canvas canvas, Point lastLocation)
        {
            //25 is arbitray we felt that this was a big enough size for the undoStack
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
                undoStack.AddFirst(new ProcessUnitUndo(command, selectedObject as IProcessUnit, canvas, lastLocation));
            }
            else
            {
            }
        }

        /// <summary>
        /// Calling this function will cause the canvas to recalculate its height and width.
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
        /// Gets how large of buffer we should allow for the edge of the canvas.  Used for
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
        /// Called whenever a user updates table data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TableDataChanged(object sender, TableDataChangedEventArgs e)
        {
            ChildrenModified();
        }

        public void ChildrenModified()
        {
            //get all child pfd elements
            var elements = from c in Children
                           where c is IPfdElement
                           select c;
            
            //build new list
            List<IPfdElement> pfdElements = new List<IPfdElement>();
            foreach (UIElement element in elements)
            {
                pfdElements.Add(element as IPfdElement);
            }
            PfdUpdatedEventArgs args = new PfdUpdatedEventArgs(pfdElements);
            PfdUpdated(this, args);
        }

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

                //create & execute the command to place the process unit onto the canvas
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

                //set the source and dest
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

                //and the dest
                var dest = from c in Children
                           where c is IProcessUnit
                           &&
                           ((c as IPfdElement).Id.CompareTo(targetNames.ElementAt(0).Destination) == 0)
                           select c;

                //set the source and dest of the stream
                s.Source = source.ElementAt(0) as IProcessUnit;
                s.Destination = dest.ElementAt(0) as IProcessUnit;

                //we can't add the streams unitl we have also built the properties table
                //so just add to local list variable
                streamObjects.Add(s);
            }

            //and finally, properties tables
            XElement tablesList = doc.Descendants("PropertiesTables").ElementAt(0);
            foreach (XElement table in tablesList.Elements())
            {
                //store the table's target
                string parentName = (string)table.Element("ParentStream");

                //create the table
                IPropertiesTable pTable = PropertiesTableFactory.TableFromXml(table);

                //find the parent on the canvas
                var parent = from c in streamObjects
                             where c.Id.CompareTo(parentName) == 0
                             select c;
                pTable.ParentStream = parent.ElementAt(0);
                parent.ElementAt(0).Table = pTable;

                //add the stream, and therefore the table to the canvas
                Commands.ICommand cmd = CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, parent.ElementAt(0), this, new Point(-1.0, -1.0));
                cmd.Execute();

                //tell the stream to redraw in order to fix any graphical glitches
                parent.ElementAt(0).UpdateStreamLocation();
            }

            //kind of a hack, but the rule checker fails durring object creation for obvious reasons.
            //In order to get around this, we're delaying the attaching of streams to process units,
            //which essentially prevents the error checker from working.  We do this at the end after
            //everthing has been created an added to the canvas
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
        /// Turns the drawing canvas into an XML object
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer)
        {
            //before writing, separate the elements based on type
            List<IProcessUnit> processUnits = new List<IProcessUnit>();
            List<IStream> streams = new List<IStream>();
            List<IPropertiesTable> propertiesTables = new List<IPropertiesTable>();
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
                    else if (element is IPropertiesTable)
                    {
                        propertiesTables.Add(element as IPropertiesTable);
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
            writer.WriteStartElement("PropertiesTables");
            foreach (IPfdElement element in propertiesTables)
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
        /// Helper function used to get the right type of XML Serializer
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
            else if (element is ChemicalStreamPropertiesTable)
            {
                return new XmlSerializer(typeof(ChemicalStreamPropertiesTable));
            }
            return new XmlSerializer(typeof(NullSerializer));
        }
        #endregion

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

            #endregion
        }
    }
}
