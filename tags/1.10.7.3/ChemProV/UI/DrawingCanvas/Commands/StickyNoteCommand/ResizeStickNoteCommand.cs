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

using ChemProV.PFD.StickyNote;

namespace ChemProV.UI.DrawingCanvas.Commands.StickyNoteCommand
{
    public class ResizeStickNoteCommand : ICommand
    {
         /// <summary>
        /// Private reference to our drawing_canvas.  Needed to add the new object to the drawing space
        /// </summary>
        private Panel canvas;

        public Panel Canvas
        {
            get { return canvas; }
            set { canvas = value; }
        }

        /// <summary>
        /// Reference to the process unit to add the the drawing_canvas.
        /// </summary>
        private StickyNote resizingStickyNote;

        public StickyNote ResizingStickyNote
        {
            get { return resizingStickyNote; }
            set { resizingStickyNote = value; }
        }

        /// <summary>
        /// Reference to the target location where we'd like to add the process unit
        /// </summary>
        private Point location;

        public Point Location
        {
            get { return location; }
            set { location = value; }
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
                instance = new ResizeStickNoteCommand();
            }
            return instance;
        }

        /// <summary>
        /// Constructor method
        /// </summary>
        private ResizeStickNoteCommand()
        {
        }

        public bool Execute()
        {
            UIElement puAsUiElement = resizingStickyNote;

            Point topleft = new Point();

            topleft.X = (double)puAsUiElement.GetValue(System.Windows.Controls.Canvas.LeftProperty);
            topleft.Y = (double)puAsUiElement.GetValue(System.Windows.Controls.Canvas.TopProperty);

            Point newSize = new Point(location.X - topleft.X, location.Y - topleft.Y);

            if (newSize.X < 0 )
            {
                newSize.X = 0;
            }
            if(newSize.Y < 0)
            {
                newSize.Y = 0;
            }

            resizingStickyNote.Width = newSize.X;
            resizingStickyNote.Height = newSize.Y;

            return true;
        }
    }
}
