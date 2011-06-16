using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using ChemProV.UI.DrawingCanvas.Commands;
using ChemProV.PFD;
using ChemProV.PFD.Streams;
using ChemProV.PFD.ProcessUnits;

namespace ChemProV.UI.DrawingCanvas
{
    public class StreamUndo : SavedStateObject
    {

        //The command that was issued note we must do the opposite of this command.
        private CanvasCommands commandIssed;

        public CanvasCommands CommandIssed
        {
            get { return commandIssed; }
        }

        //Stream we are saving
        private IStream streamManipulated;

        public IStream StreamManipulated
        {
            get { return streamManipulated; }
        }

        //The canvas used to do this on
        private Canvas theCanvasUsed;

        public Canvas TheCanvasUsed
        {
            get { return theCanvasUsed; }
        }

        private IProcessUnit source;
        
        /// <summary>
        /// The source of the stream
        /// </summary>
        public IProcessUnit Source
        {
            get { return source; }
        }

        private ProcessUnitUndo sourceUndo;

        public ProcessUnitUndo SourceUndo
        {
            get { return sourceUndo; }
        }

        private ProcessUnitUndo destinationUndo;

        public ProcessUnitUndo DestinationUndo
        {
            get { return destinationUndo; }
        }

        
        private IProcessUnit destination;

        /// <summary>
        /// The destination of the stream
        /// </summary>
        public IProcessUnit Destination
        {
            get { return destination; }
        }

        private Point location;

        public Point Location
        {
            get { return location; }
        }

        public StreamUndo(CanvasCommands commandIssued, IStream objectManipulated, Canvas CanvasUsed, IProcessUnit source, IProcessUnit destination, Point location)
        {
            this.commandIssed = commandIssued;
            this.streamManipulated = objectManipulated;
            this.theCanvasUsed = CanvasUsed;
            this.source = source;
            this.destination = destination;

            if (commandIssed == CanvasCommands.MoveHead)
            {
                this.location = new Point(location.X + (destination as UserControl).ActualWidth / 2, location.Y + (destination as UserControl).ActualHeight / 2);
            }
            else if (commandIssed == CanvasCommands.MoveHead)
            {
                this.location = new Point(location.X + (source as UserControl).ActualWidth / 2, location.Y + (source as UserControl).ActualHeight / 2);
            }
            else
            {
                this.location = location;
            }
                

            if (commandIssed == CanvasCommands.RemoveFromCanvas || commandIssed == CanvasCommands.AddToCanvas)
            {
                Point temporayPoint;
                if (objectManipulated.Source is TemporaryProcessUnit)
                {
                    temporayPoint = new Point((double)(objectManipulated.Source as UIElement).GetValue(Canvas.LeftProperty), (double)(objectManipulated.Source as UIElement).GetValue(Canvas.TopProperty));
                    this.sourceUndo = new ProcessUnitUndo(commandIssed, objectManipulated.Source as IProcessUnit, CanvasUsed, temporayPoint);
                }
                if (objectManipulated.Destination is TemporaryProcessUnit)
                {
                    temporayPoint = new Point((double)(objectManipulated.Destination as UIElement).GetValue(Canvas.LeftProperty), (double)(objectManipulated.Destination as UIElement).GetValue(Canvas.TopProperty));
                    this.destinationUndo = new ProcessUnitUndo(commandIssed, objectManipulated.Destination as IProcessUnit, CanvasUsed, temporayPoint);
                }
            }
        }
    }
}
