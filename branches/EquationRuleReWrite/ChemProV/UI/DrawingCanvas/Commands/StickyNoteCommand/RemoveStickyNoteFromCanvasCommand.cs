/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ChemProV.PFD.StickyNote;

namespace ChemProV.UI.DrawingCanvas.Commands.StickyNoteCommand
{
    public class RemoveStickyNoteFromCanvasCommand : ICommand
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
        private StickyNote removeStickyNote;

        public StickyNote RemoveStickyNote
        {
            get { return removeStickyNote; }
            set { removeStickyNote = value; }
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
                instance = new RemoveStickyNoteFromCanvasCommand();
            }
            return instance;
        }

        /// <summary>
        /// Constructor method
        /// </summary>
        private RemoveStickyNoteFromCanvasCommand()
        {
        }

        public bool Execute()
        {
            UIElement puAsUiElement = removeStickyNote as UIElement;
            puAsUiElement.SetValue(System.Windows.Controls.Canvas.ZIndexProperty, 2);
            puAsUiElement.MouseLeftButtonDown -= new MouseButtonEventHandler((canvas as DrawingCanvas).MouseLeftButtonDownHandler);
            puAsUiElement.MouseLeftButtonUp -= new MouseButtonEventHandler((canvas as DrawingCanvas).MouseLeftButtonUpHandler);
            puAsUiElement.MouseRightButtonDown -= new MouseButtonEventHandler((canvas as DrawingCanvas).MouseRightButtonDownHandler);
            puAsUiElement.MouseRightButtonUp -= new MouseButtonEventHandler((canvas as DrawingCanvas).MouseRightButtonUpHandler);
            puAsUiElement.MouseEnter -= new MouseEventHandler((canvas as DrawingCanvas).IProcessUnit_MouseEnter);
            puAsUiElement.MouseLeave -= new MouseEventHandler((canvas as DrawingCanvas).IProcessUnit_MouseLeave);
            canvas.Children.Remove(removeStickyNote);
            (canvas as DrawingCanvas).HoveringOverStickyNote = null;
            return true;
        }
    }
}