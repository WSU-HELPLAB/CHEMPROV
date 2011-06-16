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
using ChemProV.UI.DrawingCanvas.Commands.ProcessUnit;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.PFD.StickyNote;

namespace ChemProV.UI.DrawingCanvas.States
{
    public class PlacingState : IState
    {

        private DrawingCanvas canvas;
        private CommandFactory commandFactory;
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
        /// When the mouse enters the drawing canvas we need to make the new pallet item and change it to the moving state since now we are
        /// just moving the element around
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseEnter(object sender, MouseEventArgs e)
        {
            if (canvas.SelectedPaletteItem is IProcessUnit || canvas.SelectedPaletteItem is StickyNote)
            {
                canvas.SelectedElement = canvas.SelectedPaletteItem;
                CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, canvas.SelectedPaletteItem, canvas, e.GetPosition(canvas)).Execute();
                canvas.saveState(CanvasCommands.AddToCanvas, canvas.SelectedElement, canvas, new Point());
                canvas.placedNewTool();
                canvas.CurrentState = canvas.MovingState;
            }
        }

        /// <summary>
        /// On mouse down for streams we need to place the source then call moving state on the head since we are just moving the
        /// head around now
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (canvas.SelectedPaletteItem is IStream)
            {
                if (canvas.HoveringOver as IProcessUnit != null)
                {
                    if ((canvas.HoveringOver as IProcessUnit).IsAcceptingOutgoingStreams)
                    {
                        //Make source
                        CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, canvas.SelectedPaletteItem, canvas, e.GetPosition(canvas)).Execute();
                        canvas.saveState(CanvasCommands.AddToCanvas, (canvas.SelectedElement as StreamEnd).Stream, canvas, new Point());
                        canvas.placedNewTool();
                        (canvas as DrawingCanvas).ChildrenModified();
                        canvas.CurrentState = canvas.MovingState;
                    }
                    else
                    {
                        //pretend like we placed it.
                        canvas.placedNewTool();
                        canvas.CurrentState = canvas.MovingState;
                    }
                }
                else
                {
                    //Make source
                    CommandFactory.CreateCommand(CanvasCommands.AddToCanvas, canvas.SelectedPaletteItem, canvas, e.GetPosition(canvas)).Execute();
                    canvas.saveState(CanvasCommands.AddToCanvas, (canvas.SelectedElement as StreamEnd).Stream, canvas, new Point());
                    canvas.placedNewTool();
                    canvas.CurrentState = canvas.MovingState;
                }
            }
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (canvas.HoveringOver != null)
            {
                if ((canvas.HoveringOver as IProcessUnit).IsAcceptingOutgoingStreams)
                {
                    (canvas.HoveringOver as GenericProcessUnit).SetBorderColor(new SolidColorBrush (Colors.Green));
                }
                else
                {
                    (canvas.HoveringOver as GenericProcessUnit).SetBorderColor(new SolidColorBrush (Colors.Red));
                }
            }
        }

        #region Unused Mouse Events
        public void MouseLeave(object sender, MouseEventArgs e)
        {
        }

        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        public void MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        public void MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        public void MouseWheel(object sender, MouseEventArgs e)
        {
        }
        #endregion

        #endregion
    }
}
