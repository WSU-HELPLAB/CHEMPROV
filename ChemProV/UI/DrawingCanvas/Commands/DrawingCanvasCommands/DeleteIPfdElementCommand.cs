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
using ChemProV.PFD.Streams.PropertiesWindow;

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
                Point previousLocation = new Point((double)(drawing_canvas.SelectedElement as UIElement).GetValue(Canvas.LeftProperty), (double)(drawing_canvas.SelectedElement as UIElement).GetValue(Canvas.TopProperty));
                drawing_canvas.saveState(CanvasCommands.RemoveFromCanvas, drawing_canvas.SelectedElement, drawing_canvas, previousLocation);
            }
            else if (drawing_canvas.SelectedElement is IPropertiesWindow)
            {
                drawing_canvas.SelectedElement = (drawing_canvas.SelectedElement as IPropertiesWindow).ParentStream;
                Point previousLocation = new Point((double)(drawing_canvas.SelectedElement as UIElement).GetValue(Canvas.LeftProperty), (double)(drawing_canvas.SelectedElement as UIElement).GetValue(Canvas.TopProperty));
                drawing_canvas.saveState(CanvasCommands.RemoveFromCanvas, drawing_canvas.SelectedElement, drawing_canvas, previousLocation);
            }
            else
            {
                Point previousLocation = new Point((double)(drawing_canvas.SelectedElement as UIElement).GetValue(Canvas.LeftProperty), (double)(drawing_canvas.SelectedElement as UIElement).GetValue(Canvas.TopProperty));
                drawing_canvas.saveState(CanvasCommands.RemoveFromCanvas, drawing_canvas.SelectedElement, drawing_canvas, previousLocation);
            }

            //this stops a Heat Stream from a Heat Exchanger from being deleted
            if (drawing_canvas.SelectedElement is HeatStream)
            {
                if (ProcessUnitFactory.GetProcessUnitType(((drawing_canvas.SelectedElement as HeatStream).Destination as IProcessUnit)) == ProcessUnitType.HeatExchanger)
                {
                    return false;
                }
            }

            CommandFactory.CreateCommand(CanvasCommands.RemoveFromCanvas, drawing_canvas.SelectedElement, drawing_canvas, new Point()).Execute();
            return true;
        }
    }
}