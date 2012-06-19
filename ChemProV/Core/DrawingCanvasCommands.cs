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
using ChemProV.PFD.Undos;

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
            if (element is PFD.StickyNote.StickyNoteControl)
            {
                // Case 1: deleting a sticky note
                (element as PFD.StickyNote.StickyNoteControl).DeleteWithUndo(canvas);
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
            else if (element is HeatExchanger)
            {
                DeleteHEWU(element as HeatExchanger, canvas);
            }
            else if (element is GenericProcessUnit)
            {
                DeleteProcessUnitWithUndo(element as GenericProcessUnit, canvas);
            }
            else
            {
                // Generic case - just remove whatever's selected with an undo to re-add it
                canvas.AddUndo(new UndoRedoCollection("Undo", new AddToCanvas(element, canvas)));
                canvas.Children.Remove(element);
            }

            // If we just deleted the item that was just selected, we should set the selected 
            // element to null
            if (object.ReferenceEquals(element, canvas.SelectedElement))
            {
                canvas.SelectedElement = null;
            }
        }

        private static void DeleteHEWU(HeatExchanger he, DrawingCanvas canvas)
        {
            int i;
            
            // Heat exchangers with utilities must also delete the heat stream that's incoming
            // Normally the heat stream is at index 0 among the incoming streams, but it seems like 
            // this does not always hold true when loading from files so it's just safer to search 
            // for it
            HeatStream heatStream = null;
            foreach (IStream incomingStream in he.IncomingStreams)
            {
                if (incomingStream is HeatStream)
                {
                    heatStream = incomingStream as HeatStream;
                    break;
                }
            }

            List<IUndoRedoAction> undos = new List<IUndoRedoAction>();
            undos.Add(new AddToCanvas(he, canvas));
            undos.Add(new AddToCanvas(heatStream, canvas));
            undos.Add(new AddToCanvas(heatStream.SourceDragIcon, canvas));
            undos.Add(new AddToCanvas(heatStream.DestinationDragIcon, canvas));
            undos.Add(new AddToCanvas(heatStream.Table as UIElement, canvas));

            // We need to check if the attached stream has a source
            if (null != heatStream.Source)
            {
                // Detach with undo
                undos.Add(new AttachOutgoingStream(heatStream.Source, heatStream));
                heatStream.Source.DettachOutgoingStream(heatStream);
            }

            // Delete all comment sticky notes for the process unit
            for (i = 0; i < he.CommentCount; i++)
            {
                PFD.StickyNote.StickyNoteControl sn = he.GetCommentAt(i) as PFD.StickyNote.StickyNoteControl;

                // We will not remove the comment from the collection, but we must remove it
                // from the canvas. Comment sticky notes have both a sticky note control and 
                // a connecting line that have to be removed.
                undos.Add(new AddToCanvas(sn, canvas));
                undos.Add(new AddToCanvas(sn.LineToParent, canvas));

                canvas.RemoveChild(sn);
                canvas.RemoveChild(sn.LineToParent);
            }

            // Delete all comment sticky notes for the stream
            ICommentCollection cc = heatStream as ICommentCollection;
            if (null != cc)
            {
                for (i = 0; i < cc.CommentCount; i++)
                {
                    PFD.StickyNote.StickyNoteControl sn = cc.GetCommentAt(i) as PFD.StickyNote.StickyNoteControl;

                    // We will not remove the comment from the collection, but we must remove it
                    // from the canvas. Comment sticky notes have both a sticky note control and 
                    // a connecting line that have to be removed.
                    undos.Add(new AddToCanvas(sn, canvas));
                    undos.Add(new AddToCanvas(sn.LineToParent, canvas));

                    canvas.RemoveChild(sn);
                    canvas.RemoveChild(sn.LineToParent);
                }
            }

            // Detach all outgoing streams and make undos
            for (i = 0; i < he.OutgoingStreams.Count; i++)
            {
                IStream s = he.OutgoingStreams[i];
                undos.Add(new SetStreamSource(s, he, null, (s as AbstractStream).SourceDragIcon.Location));
                s.Source = null;
            }

            // Detach all incoming streams and make undos
            for (i = 0; i < he.IncomingStreams.Count; i++)
            {
                IStream s = he.IncomingStreams[i];
                undos.Add(new SetStreamDestination(s, he, null));
                s.Destination = null;
            }

            canvas.AddUndo(new UndoRedoCollection(
                "Undo deletion of heat exchanger with utility", undos.ToArray()));

            // Remove the pieces from the canvas
            canvas.RemoveChild(he);
            canvas.RemoveChild(heatStream);
            canvas.RemoveChild(heatStream.SourceDragIcon);
            canvas.RemoveChild(heatStream.DestinationDragIcon);
            canvas.RemoveChild(heatStream.Table as UIElement);
        }

        private static void DeleteProcessUnitWithUndo(GenericProcessUnit pu, DrawingCanvas canvas)
        {
            List<IUndoRedoAction> undos = new List<IUndoRedoAction>();
            undos.Add(new AddToCanvas(pu, canvas));

            // Detach all incoming streams and make undos
            int i;
            for (i = 0; i < pu.IncomingStreams.Count; i++)
            {
                IStream s = pu.IncomingStreams[i];
                undos.Add(new SetStreamDestination(s, pu, null));
                s.Destination = null;
            }

            // Detach all outgoing streams and make undos
            for (i = 0; i < pu.OutgoingStreams.Count; i++)
            {
                IStream s = pu.OutgoingStreams[i];
                undos.Add(new SetStreamSource(s, pu, null, (s as AbstractStream).SourceDragIcon.Location));
                s.Source = null;
            }

            // Delete all comment sticky notes
            ICommentCollection cc = pu as ICommentCollection;
            if (null != cc)
            {
                for (i = 0; i < cc.CommentCount; i++)
                {
                    PFD.StickyNote.StickyNoteControl sn = cc.GetCommentAt(i) as PFD.StickyNote.StickyNoteControl;
                    
                    // We will not remove the comment from the collection, but we must remove it
                    // from the canvas. Comment sticky notes have both a sticky note control and 
                    // a connecting line that have to be removed.
                    undos.Add(new AddToCanvas(sn, canvas));
                    undos.Add(new AddToCanvas(sn.LineToParent, canvas));

                    canvas.RemoveChild(sn);
                    canvas.RemoveChild(sn.LineToParent);
                }
            }

            canvas.AddUndo(new UndoRedoCollection(
                "Undo deletion of process unit", undos.ToArray()));

            // Remove the process unit
            canvas.RemoveChild(pu);
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
            // Special case: the old version (before I rewrote a bunch of logic) didn't let 
            // you delete heat streams whose destination was a heat exchanger with utility.
            // I will keep this functionality.
            if (stream is HeatStream && stream.Destination is HeatExchanger)
            {
                return;
            }

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
                stream.Destination.DettachIncomingStream(stream);
            }

            // Delete all comment sticky notes
            int i;
            ICommentCollection cc = stream as ICommentCollection;
            if (null != cc)
            {
                for (i = 0; i < cc.CommentCount; i++)
                {
                    PFD.StickyNote.StickyNoteControl sn = cc.GetCommentAt(i) as PFD.StickyNote.StickyNoteControl;

                    // We will not remove the comment from the collection, but we must remove it
                    // from the canvas. Comment sticky notes have both a sticky note control and 
                    // a connecting line that have to be removed.
                    actions.Add(new AddToCanvas(sn, canvas));
                    actions.Add(new AddToCanvas(sn.LineToParent, canvas));

                    canvas.RemoveChild(sn);
                    canvas.RemoveChild(sn.LineToParent);
                }
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
