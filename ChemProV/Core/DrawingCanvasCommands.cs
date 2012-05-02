/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

// Since I'm undertaking the massive effort of refactoring this application and in 
// the process I'm getting rid of the ICommand and CommandFactory stuff, I'm 
// creating this static class to replace some of that functionality.

using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ChemProV.PFD;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.UI.DrawingCanvas;

namespace ChemProV.Core
{
    /// <summary>
    /// Static class to execute a variety of common commands associated with the drawing canvas in 
    /// ChemProV. All methods that alter anything will create an appropriate undo.
    /// </summary>
    public static class DrawingCanvasCommands
    {
        /// <summary>
        /// Deletes the specified element from the drawing canvas and creates an undo that will 
        /// restore it. Recognizes the type of the element and takes appropriate actions based 
        /// on whether it is a stream, process unit, etc.
        /// </summary>
        public static void DeleteElement(DrawingCanvas canvas, UIElement element)
        {
            // If the canvas is read-only then let the user know
            if (canvas.IsReadOnly)
            {
                MessageBox.Show("Canvas is currently in read-only mode so items cannot be deleted");
                return;
            }

            // There are several different possibilities for what we're about to delete.
            if (element is PFD.StickyNote.StickyNote)
            {
                // Case 1: deleting a sticky note
                (element as PFD.StickyNote.StickyNote).DeleteWithUndo(canvas);
            }
            else if (element is AbstractStream)
            {
                // Case 2: deleting a stream

                DeleteStreamWithUndo((AbstractStream)element, canvas);
            }
            else if (element is DraggableStreamEndpoint)
            {
                // Case 3: Selected element is a stream drag handle, in which case we are also 
                // deleting a stream

                DraggableStreamEndpoint dse = element as DraggableStreamEndpoint;
                DeleteStreamWithUndo((AbstractStream)dse.ParentStream, canvas);
                return;
            }
            else if (element is PFD.Streams.PropertiesWindow.IPropertiesWindow)
            {
                // Case 4: Selected element is a stream properties window, in which case we are 
                // also deleting a stream
                PFD.Streams.PropertiesWindow.IPropertiesWindow win = element as
                    PFD.Streams.PropertiesWindow.IPropertiesWindow;

                DeleteStreamWithUndo(win.ParentStream as AbstractStream, canvas);
            }
            else
            {
                // Generic case - just remove whatever's selected with an undo to re-add it
                canvas.AddUndo(new PFD.UndoRedoCollection("Undo",
                    new PFD.Undos.AddToCanvas(element, canvas)));
                canvas.Children.Remove(element);
            }

            // If we just deleted the item that was just selected, so we should set the selected 
            // element to null
            if (object.ReferenceEquals(element, canvas.SelectedElement))
            {
                canvas.SelectedElement = null;
            }
        }
        
        public static void DeleteSelectedElement(DrawingCanvas canvas)
        {
            // If there's nothing selected then we just go to null state and return
            if (null == canvas.SelectedElement)
            {
                canvas.CurrentState = null;
                return;
            }

            DeleteElement(canvas, (UIElement)canvas.SelectedElement);
        }

        private static void DeleteStreamWithUndo(AbstractStream stream, DrawingCanvas canvas)
        {
            // Case 1: It's not connected to any process units
            if (null == stream.Source && null == stream.Destination)
            {
                // Start by creating an undo to add it back to the canvas. Note that there are 
                // several parts for a stream that get added separately: the stream control itself, 
                // two draggable endpoints, and the table.
                canvas.AddUndo(new PFD.UndoRedoCollection("Undo deletion of stream",
                    new PFD.Undos.AddToCanvas(stream, canvas),
                    new PFD.Undos.AddToCanvas(stream.SourceDragIcon, canvas),
                    new PFD.Undos.AddToCanvas(stream.DestinationDragIcon, canvas),
                    new PFD.Undos.AddToCanvas(stream.Table as UIElement, canvas)));

                // Remove the pieces from the canvas
                canvas.RemoveChild(stream);
                canvas.RemoveChild(stream.SourceDragIcon);
                canvas.RemoveChild(stream.DestinationDragIcon);
                canvas.RemoveChild(stream.Table as UIElement);

                return;
            }

            // Case 2 is that it IS connected to one or two process units. In this case we 
            // have to disconnect it, remove it from the canvas and make sure that the undo 
            // adds it back and reconnects it properly.

            // We need to build a list of undos. Some actions are shared by all cases.
            List<IUndoRedoAction> actions = new List<IUndoRedoAction>();
            // In all cases we will need to add back the components of the stream
            actions.AddRange(new IUndoRedoAction[]{
                new PFD.Undos.AddToCanvas(stream, canvas),
                new PFD.Undos.AddToCanvas(stream.SourceDragIcon, canvas),
                new PFD.Undos.AddToCanvas(stream.DestinationDragIcon, canvas),
                new PFD.Undos.AddToCanvas(stream.Table as UIElement, canvas)});

            if (null != stream.Source)
            {
                // If the stream has a non-null source then we need to detach it and 
                // make sure the undo would reattach it
                actions.Add(new PFD.Undos.AttachOutgoingStream(stream.Source, stream));

                // Do the detachment so that the process unit (which is staying around) won't 
                // have an outgoing stream that's been deleted
                stream.Source.DettachOutgoingStream(stream);
            }
            if (null != stream.Destination)
            {
                // If the stream has a non-null destination then we need to detach it and 
                // make sure the undo would reattach it
                actions.Add(new PFD.Undos.AttachIncomingStream(stream.Destination, stream));

                // Do the detachment so that the process unit (which is staying around) won't 
                // have an incoming stream that's been deleted
                stream.Source.DettachIncomingStream(stream);
            }

            // Now we actually remove the stream. Note that this stream object will hold onto 
            // any source or destination stream references that it has
            canvas.RemoveChild(stream);
            canvas.RemoveChild(stream.SourceDragIcon);
            canvas.RemoveChild(stream.DestinationDragIcon);
            canvas.RemoveChild(stream.Table as UIElement);

            // Add the undo that we built
            canvas.AddUndo(new UndoRedoCollection("Undo deletion of stream", actions.ToArray()));
        }
    }
}
