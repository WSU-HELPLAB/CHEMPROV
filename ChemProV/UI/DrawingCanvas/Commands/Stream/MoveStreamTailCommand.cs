/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Windows;
using System.Windows.Controls;

using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;

namespace ChemProV.UI.DrawingCanvas.Commands.Stream
{
    public class MoveStreamTailCommand : ICommand
    {
        private IStream streamToMove;

        /// <summary>
        /// Reference to the stream we are manipulating.
        /// </summary>
        public IStream StreamToMove
        {
            get { return streamToMove; }
            set { streamToMove = value; }
        }

        /// <summary>
        /// Reference to the target location where we'd like to add the process unit
        /// </summary>
        private Point currentMouseLocation;

        public Point CurrentMouseLocation
        {
            get { return currentMouseLocation; }
            set { currentMouseLocation = value; }
        }

        private Point previousMouseLocation;

        public Point PreviousMouseLocation
        {
            get { return previousMouseLocation; }
            set { previousMouseLocation = value; }
        }

        private DrawingCanvas drawingcanvas;

        public DrawingCanvas Drawingcanvas
        {
            get { return drawingcanvas; }
            set { drawingcanvas = value; }
        }

        /// <summary>
        /// Private reference to our drawing_canvas.  Needed to add the new object to the drawing space
        /// </summary>
        private Panel canvas;

        public Panel Canvas
        {
            get { return canvas; }
            set { canvas = value; }
        }

        private static ICommand instance;

        /// <summary>
        /// Used to get at the single instance of this object
        /// </summary>
        /// <returns></returns>
        public static ICommand GetInstance()
        {
            if (instance == null)
            {
                instance = new MoveStreamTailCommand();
            }
            return instance;
        }

        /// <summary>
        /// Constructor method
        /// </summary>
        private MoveStreamTailCommand()
        {
        }

        /// <summary>
        /// This checks to see if we are connected to a temp process unit if we are we delete it if not then we just let
        /// the process unit know we are dettaching from it. Then we check to see if we are over a process unit if we are
        /// we attach to it, if we can, if we are not over a process unit we make a new source at the location given.
        /// </summary>
        public bool Execute()
        {
            if ((canvas as DrawingCanvas).IsReadOnly)
            {
                CommandFactory.CreateCommand(CanvasCommands.MoveHead, streamToMove.Source, canvas, currentMouseLocation, previousMouseLocation).Execute();
                return true;
            }
            else
            {
                if (streamToMove.Source is IProcessUnit && !(streamToMove.Source is TemporaryProcessUnit))
                {
                    (streamToMove.Source as IProcessUnit).DettachOutgoingStream(streamToMove);
                    streamToMove.Source = null;
                }

                IProcessUnit hoveringOver = ((canvas as DrawingCanvas).HoveringOver as IProcessUnit);
                if (hoveringOver != null)
                {
                    if (streamToMove.Source is TemporaryProcessUnit || streamToMove.Source == null)
                    {
                        CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, streamToMove.Source, canvas, currentMouseLocation).Execute();
                        streamToMove.Source = null;
                    }
                    if (hoveringOver.IsAcceptingOutgoingStreams(streamToMove))
                    {
                        streamToMove.Source = (canvas as DrawingCanvas).HoveringOver as IProcessUnit;
                        ((canvas as DrawingCanvas).HoveringOver as IProcessUnit).AttachOutgoingStream(streamToMove);
                        (hoveringOver as GenericProcessUnit).SetBorderColor(ProcessUnitBorderColor.AcceptingStreams);
                        (streamToMove as AbstractStream).SourceRectangleVisbility = true;
                        return true;
                    }
                    else
                    {
                        (hoveringOver as GenericProcessUnit).SetBorderColor(ProcessUnitBorderColor.NotAcceptingStreams);
                        return false;
                    }
                }
                if (streamToMove.Source == null)
                {
                    IProcessUnit source = ProcessUnitFactory.ProcessUnitFromUnitType(ProcessUnitType.Source);
                    CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, source, canvas, currentMouseLocation).Execute();
                    streamToMove.Source = source;
                    (streamToMove as AbstractStream).SourceRectangleVisbility = false;
                    source.AttachOutgoingStream(streamToMove);
                }
                else if (streamToMove.Source is TemporaryProcessUnit)
                {
                    //use IProcessUnitMove to move the stream destination
                    //NOTE: MoveHead isn't wrong because we are talking about the source icon itself and it only has a head.
                    CommandFactory.CreateCommand(CanvasCommands.MoveHead, streamToMove.Source, canvas, currentMouseLocation, previousMouseLocation).Execute();
                }
                return true;
            }
        }
    }
}