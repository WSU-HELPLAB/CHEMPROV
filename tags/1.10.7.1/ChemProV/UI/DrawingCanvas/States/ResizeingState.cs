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
using ChemProV.PFD.StickyNote;

namespace ChemProV.UI.DrawingCanvas.States
{
    public class ResizeingState : IState
    {
        private DrawingCanvas canvas;

        public ResizeingState(DrawingCanvas c)
        {
            canvas = c;
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (canvas.SelectedElement is StickyNote)
            {
                CommandFactory.CreateCommand(CanvasCommands.Resize, canvas.SelectedElement as StickyNote, canvas, e.GetPosition(canvas)).Execute();
            }
        }

        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            canvas.Cursor = Cursors.Arrow;
            canvas.CurrentState = canvas.SelectedState;
        }

        #region Unused Mouse Events
        public void MouseEnter(object sender, MouseEventArgs e)
        {
            
        }

        public void MouseLeave(object sender, MouseEventArgs e)
        {
            
        }

        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
    }
}
