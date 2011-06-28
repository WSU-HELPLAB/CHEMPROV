/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System.Windows;
using System.Windows.Controls;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.UI.DrawingCanvas.Commands;

namespace ChemProV.UI.DrawingCanvas
{
    /// <summary>
    /// This saves the state of stream which is used to undo a stream action
    /// </summary>
    public class StreamUndo : SavedStateObject
    {
        private CanvasCommands commandIssed;

        /// <summary>
        /// The command that was issued note we must do the opposite of this command.
        /// </summary>
        public CanvasCommands CommandIssed
        {
            get { return commandIssed; }
        }

        private IStream streamManipulated;

        /// <summary>
        /// Stream we are saving
        /// </summary>
        public IStream StreamManipulated
        {
            get { return streamManipulated; }
        }

        private Canvas theCanvasUsed;

        /// <summary>
        /// The drawing_canvas that was used
        /// </summary>
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

        /// <summary>
        /// A reference to the ProcessUnitUndo which was made to save the source of the stream
        /// </summary>
        public ProcessUnitUndo SourceUndo
        {
            get { return sourceUndo; }
        }

        private ProcessUnitUndo destinationUndo;

        /// <summary>
        /// A reference to the ProcessUnitUndo which was made to save the source of the stream
        /// </summary>
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

        /// <summary>
        /// The location it was at
        /// </summary>
        public Point Location
        {
            get { return location; }
        }

        /// <summary>
        /// This undoes a command that was issued for a stream.
        /// </summary>
        /// <param name="commandIssued">The command issued</param>
        /// <param name="objectManipulated">The stream that was changed</param>
        /// <param name="CanvasUsed">The drawing_canvas</param>
        /// <param name="source">The IPU that is the source of the stream</param>
        /// <param name="destination">The IPU that is the destination of the stream</param>
        /// <param name="location">The location it was at.</param>
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
            else if (commandIssed == CanvasCommands.MoveTail)
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