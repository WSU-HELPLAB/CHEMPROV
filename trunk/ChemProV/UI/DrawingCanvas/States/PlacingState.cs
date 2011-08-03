/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Windows;
using System.Windows.Input;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.StickyNote;
using ChemProV.PFD.Streams;
using ChemProV.UI.DrawingCanvas.Commands;

namespace ChemProV.UI.DrawingCanvas.States
{
    /// <summary>
    /// This is when we are placing a new object from the Pallet. IPU is in placing state until the mouse enters the drawing
    /// drawing_canvas then it switchs to moving state.  A Stream stay in placing state until MouseLeftButtonDown is fired on drawing drawing_canvas.
    /// </summary>
    public class PlacingState : IState
    {
        private DrawingCanvas canvas;
        private CommandFactory commandFactory;

        /// <summary>
        /// This is the constructor for the Placeing State
        /// </summary>
        /// <param name="c">A reference to the Drawing Canvas</param>
        /// <param name="cf">Optional: A reference to a CommandFactory</param>
        public PlacingState(DrawingCanvas c, CommandFactory cf = null)
        {
            canvas = c;
            if (cf == null)
            {
                cf = new CommandFactory();
            }
            commandFactory = cf;
        }

        #region IState Members

        /// <summary>
        /// When the mouse enters the drawing drawing_canvas we need to make the new pallet item and change it to the moving state since now we are
        /// just moving the element around
        /// </summary>
        /// <param name="sender">Not Used</param>
        /// <param name="e">Not Used</param>
        public void MouseEnter(object sender, MouseEventArgs e)
        {
            if (canvas.SelectedPaletteItem is IProcessUnit || canvas.SelectedPaletteItem is StickyNote)
            {
                canvas.SelectedElement = canvas.SelectedPaletteItem;
                CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, canvas.SelectedPaletteItem, canvas, e.GetPosition(canvas), new Point(-1, -1)).Execute();
                canvas.saveState(CanvasCommands.AddToCanvas, canvas.SelectedElement, canvas, new Point());
                canvas.placedNewTool();
                canvas.CurrentState = canvas.MovingState;
            }
        }

        /// <summary>
        /// On mouse down for streams we need to place the source then call moving state on the head since we are just moving the
        /// head around now
        /// </summary>
        /// <param name="sender">Not Used</param>
        /// <param name="e">Not Used</param>
        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (canvas.SelectedPaletteItem is IStream)
            {
                if (canvas.HoveringOver as IProcessUnit != null)
                {
                    if ((canvas.HoveringOver as IProcessUnit).IsAcceptingOutgoingStreams(canvas.SelectedPaletteItem as IStream))
                    {
                        //Make source
                        CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, canvas.SelectedPaletteItem, canvas, e.GetPosition(canvas), new Point(-1, -1)).Execute();
                        canvas.saveState(CanvasCommands.AddToCanvas, (canvas.SelectedElement as StreamEnd).Stream, canvas, new Point());
                        canvas.placedNewTool();
                        (canvas as DrawingCanvas).PFDModified();
                        canvas.CurrentState = canvas.MovingState;
                    }
                    else
                    {
                        //pretend like we placed it.
                        canvas.placedNewTool();
                        canvas.CurrentState = canvas.NullState;
                    }
                }
                else
                {
                    //Make source
                    CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, canvas.SelectedPaletteItem, canvas, e.GetPosition(canvas), new Point(-1, -1)).Execute();
                    canvas.saveState(CanvasCommands.AddToCanvas, (canvas.SelectedElement as StreamEnd).Stream, canvas, new Point());
                    canvas.placedNewTool();
                    canvas.CurrentState = canvas.MovingState;
                }
            }
        }

        /// <summary>
        /// This checks to see if we are hovering over a process unit if so then if it is accepting streams make the border green
        /// if not then make the border red
        /// </summary>
        /// <param name="sender">Not Used</param>
        /// <param name="e">Not Used</param>
        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (canvas.HoveringOver != null)
            {
                if ((canvas.HoveringOver as IProcessUnit).IsAcceptingOutgoingStreams(canvas.SelectedPaletteItem as IStream))
                {
                    (canvas.HoveringOver as GenericProcessUnit).SetBorderColor(ProcessUnitBorderColor.AcceptingStreams);
                }
                else
                {
                    (canvas.HoveringOver as GenericProcessUnit).SetBorderColor(ProcessUnitBorderColor.NotAcceptingStreams);
                }
            }
        }

        #region Unused Mouse Events

        /// <summary>
        /// Does nothing
        /// </summary>
        /// <param name="sender">NOT USED</param>
        /// <param name="e">NOT USED</param>
        public void MouseLeave(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        /// <param name="sender">NOT USED</param>
        /// <param name="e">NOT USED</param>
        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        /// <param name="sender">NOT USED</param>
        /// <param name="e">NOT USED</param>
        public void MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        /// <param name="sender">NOT USED</param>
        /// <param name="e">NOT USED</param>
        public void MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        /// <param name="sender">NOT USED</param>
        /// <param name="e">NOT USED</param>
        public void MouseWheel(object sender, MouseEventArgs e)
        {
        }

        #endregion Unused Mouse Events

        #endregion IState Members

        public void LostMouseCapture(object sender, MouseEventArgs e)
        {
        }
    }
}