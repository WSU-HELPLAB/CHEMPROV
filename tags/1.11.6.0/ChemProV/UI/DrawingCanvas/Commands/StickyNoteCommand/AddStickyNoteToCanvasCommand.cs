/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ChemProV.PFD.StickyNote;

namespace ChemProV.UI.DrawingCanvas.Commands.StickyNoteCommand
{
    public class AddStickyNoteToCanvasCommand : ICommand
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
        private StickyNote newStickyNote;

        public StickyNote NewStickyNote
        {
            get { return newStickyNote; }
            set { newStickyNote = value; }
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
                instance = new AddStickyNoteToCanvasCommand();
            }
            return instance;
        }

        /// <summary>
        /// Constructor method
        /// </summary>
        private AddStickyNoteToCanvasCommand()
        {
        }

        public bool Execute()
        {
            UIElement puAsUiElement = newStickyNote.Header;

            //set above everything else
            newStickyNote.SetValue(System.Windows.Controls.Canvas.ZIndexProperty, 4);

            puAsUiElement.MouseLeftButtonDown += new MouseButtonEventHandler((canvas as DrawingCanvas).StickyNoteMouseLeftButtonDown);
            puAsUiElement.MouseLeftButtonUp += new MouseButtonEventHandler((canvas as DrawingCanvas).MouseLeftButtonUpHandler);
            puAsUiElement.MouseRightButtonDown += new MouseButtonEventHandler((canvas as DrawingCanvas).MouseRightButtonDownHandler);
            puAsUiElement.MouseRightButtonUp += new MouseButtonEventHandler((canvas as DrawingCanvas).MouseRightButtonUpHandler);
            puAsUiElement.CaptureMouse();

            newStickyNote.Closing += new MouseButtonEventHandler((canvas as DrawingCanvas).StickyNote_Closing);

            newStickyNote.MouseEnter += new MouseEventHandler((canvas as DrawingCanvas).newStickyNote_MouseEnter);
            newStickyNote.MouseLeave += new MouseEventHandler((canvas as DrawingCanvas).newStickyNote_MouseLeave);

            newStickyNote.Resizing += new EventHandler((canvas as DrawingCanvas).newNote_Resizing);

            canvas.Children.Add(newStickyNote);
            return true;
        }
    }
}