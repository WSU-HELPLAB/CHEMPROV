/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/
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

using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Streams.PropertiesTable;
using ChemProV.UI.DrawingCanvas;

namespace ChemProV.UI.DrawingCanvas.Commands.DrawingCanvasCommands
{
    public class DeleteIPfdElementCommand : ICommand
    {
                    /// <summary>
        /// Private reference to our drawing_drawing_canvas.  Needed to add the new object to the drawing space
        /// </summary>
        private DrawingCanvas drawing_canvas;

        public DrawingCanvas Drawing_Canvas
        {
            get { return drawing_canvas; }
            set { drawing_canvas = value; }
        }

                private static ICommand instance;

        /// <summarydrawing_canvas>
        /// Used to get at the single instance of this object
        /// </summary>
        /// <returns></returns>
        public static ICommand GetInstance()
        {
            if (instance == null)
            {
                instance = new DeleteIPfdElementCommand();
            }
            return instance;
        }

        /// <summary>
        /// Constructor method
        /// </summary>
        private DeleteIPfdElementCommand()
        {
        }


        public bool Execute()
        {
            if (drawing_canvas.SelectedElement is StreamEnd)
            {
                drawing_canvas.SelectedElement = (drawing_canvas.SelectedElement as StreamEnd).Stream;
            }
            else if (drawing_canvas.SelectedElement is IPropertiesTable)
            {
                drawing_canvas.SelectedElement = (drawing_canvas.SelectedElement as IPropertiesTable).ParentStream;
                Point previousLocation = new Point((double)(drawing_canvas.SelectedElement as UIElement).GetValue(Canvas.LeftProperty), (double)(drawing_canvas.SelectedElement as UIElement).GetValue(Canvas.TopProperty));
                drawing_canvas.saveState(CanvasCommands.RemoveFromCanvas, drawing_canvas.SelectedElement, drawing_canvas, previousLocation);
            }
            else
            {
                Point previousLocation = new Point((double)(drawing_canvas.SelectedElement as UIElement).GetValue(Canvas.LeftProperty), (double)(drawing_canvas.SelectedElement as UIElement).GetValue(Canvas.TopProperty));
                drawing_canvas.saveState(CanvasCommands.RemoveFromCanvas, drawing_canvas.SelectedElement, drawing_canvas, previousLocation);
            }
            CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, drawing_canvas.SelectedElement, drawing_canvas, new Point()).Execute();
            return true;
        }
    }
}
