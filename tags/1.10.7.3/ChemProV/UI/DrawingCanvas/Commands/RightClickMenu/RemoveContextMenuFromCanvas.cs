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

namespace ChemProV.UI.DrawingCanvas.Commands.RightClickMenu
{
    public class RemoveContextMenuFromCanvas : ICommand
    {
 /// <summary>
        /// Private reference to our drawing_canvas.  Needed to add the new object to the drawing space
        /// </summary>
        private DrawingCanvas drawing_canvas;

        public DrawingCanvas Drawing_Canvas
        {
            get { return drawing_canvas; }
            set { drawing_canvas = value; }
        }

        private ContextMenu newContextMenuToBeRemove;

        public ContextMenu ContextMenuToBeRemove
        {
            get { return newContextMenuToBeRemove; }
            set { newContextMenuToBeRemove = value; }
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
                instance = new RemoveContextMenuFromCanvas();
            }
            return instance;
        }

        /// <summary>
        /// Constructor method
        /// </summary>
        private RemoveContextMenuFromCanvas()
        {
        }

        /// <summary>
        /// Adds the process unit to the given drawing_canvas at the given point being its middle.
        /// </summary>
        public bool Execute()
        {
            drawing_canvas.Children.Remove(newContextMenuToBeRemove);
            drawing_canvas.NewContextMenu = null;
            return true;
        }
    }
}