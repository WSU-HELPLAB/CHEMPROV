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

using System.Collections.Generic;
using System.Windows;
using ChemProV.Logic;
using ChemProV.Logic.Undos;
using ChemProV.UI;

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

            // Get a reference to the workspace
            Workspace ws = canvas.GetWorkspace();

            // There are several different possibilities for what we're about to delete.
            if (element is StickyNoteControl)
            {
                // Case 1: deleting a sticky note
                (element as StickyNoteControl).DeleteWithUndo(canvas);
            }
            else if (element is ChemProV.PFD.Streams.StreamControl)
            {
                // Case 2: deleting a stream

                DeleteStreamWithUndo(element as ChemProV.PFD.Streams.StreamControl, canvas);
            }
            else if (element is PFD.Streams.PropertiesWindow.IPropertiesWindow)
            {
                // Intentionally do nothing in this case. If the user has focus on a properties window 
                // then they would probably hit the delete key to delete text in one of the text boxes. 
                // We do NOT want this to delete the stream.
            }
            else if (element is ProcessUnitControl)
            {
                if ((element as ProcessUnitControl).ProcessUnit is HeatExchangerWithUtility)
                {
                    DeleteHEWU(element as ProcessUnitControl, canvas);
                }
                else
                {
                    DeleteProcessUnitWithUndo(element as ProcessUnitControl, canvas);
                }
            }
            else
            {
                // We don't recognize the object so don't do anything
                return;
            }

            // If we just deleted the item that was just selected, we should set the selected 
            // element to null
            if (object.ReferenceEquals(element, canvas.SelectedElement))
            {
                canvas.SelectedElement = null;
            }
        }

        // Refactoring on this method is done (logic-wise, could use some cleanup otherwise)
        private static void DeleteHEWU(ProcessUnitControl he, DrawingCanvas canvas)
        {
            int i;
            Workspace ws = canvas.GetWorkspace();
            
            // Heat exchangers with utilities must also delete the heat stream that's incoming
            // Normally the heat stream is at index 0 among the incoming streams, but it seems like 
            // this does not always hold true when loading from files so it's just safer to search 
            // for it
            HeatStream heatStream = null;
            foreach (AbstractStream incomingStream in he.ProcessUnit.IncomingStreams)
            {
                if (incomingStream is HeatStream)
                {
                    heatStream = incomingStream as HeatStream;
                    break;
                }
            }
            PFD.Streams.StreamControl heatStreamControl =
                canvas.GetStreamControl(heatStream) as PFD.Streams.StreamControl;

            List<IUndoRedoAction> undos = new List<IUndoRedoAction>();
            undos.Add(new Logic.Undos.AddToWorkspace(heatStream));
            undos.Add(new Logic.Undos.AddToWorkspace(he.ProcessUnit));

            // We need to check if the attached stream has a source
            if (null != heatStream.Source)
            {
                // Detach with undo
                undos.Add(new Logic.Undos.AttachOutgoingStream(heatStream.Source, heatStream));
                heatStream.Source.DetachOutgoingStream(heatStream);
            }

            // Detach all outgoing streams and make undos
            foreach (AbstractStream s in he.ProcessUnit.OutgoingStreams)
            {
                undos.Add(new Logic.Undos.SetStreamSource(s, he.ProcessUnit, null,
                    s.SourceLocation));
                    //(s as AbstractStream).SourceDragIcon.Location));
                s.Source = null;
            }

            // Detach all incoming streams and make undos
            foreach (AbstractStream s in he.ProcessUnit.IncomingStreams)
            {
                undos.Add(new Logic.Undos.SetStreamDestination(s, he.ProcessUnit, null,
                    s.DestinationLocation));
                s.Destination = null;
            }

            ws.AddUndo(new UndoRedoCollection(
                "Undo deletion of heat exchanger with utility", undos.ToArray()));

            // Tell the process unit and heat stream to remove themselves from the canvas
            he.RemoveSelfFromCanvas(canvas);
            heatStreamControl.RemoveSelfFromCanvas(canvas);
        }

        // Refactoring on this method is done
        private static void DeleteProcessUnitWithUndo(ProcessUnitControl pu, DrawingCanvas canvas)
        {
            Workspace ws = canvas.GetWorkspace();

            // Initialize the list of undos
            List<IUndoRedoAction> undos = new List<IUndoRedoAction>();
            undos.Add(new ChemProV.Logic.Undos.AddToWorkspace(pu.ProcessUnit));

            // Detach all incoming streams and make undos
            foreach (AbstractStream s in pu.ProcessUnit.IncomingStreams)
            {
                undos.Add(new SetStreamDestination(s, pu.ProcessUnit, null, s.DestinationLocation));
                s.Destination = null;
            }

            // Detach all outgoing streams and make undos
            foreach (AbstractStream s in pu.ProcessUnit.OutgoingStreams)
            {
                undos.Add(new SetStreamSource(s, pu.ProcessUnit, null, s.SourceLocation));
                s.Source = null;
            }

            // Remove the process unit from the workspace. Event handlers will update UI.
            ws.RemoveProcessUnit(pu.ProcessUnit);

            // Finalize the undo
            ws.AddUndo(new UndoRedoCollection("Undo deletion of process unit", undos.ToArray()));
        }

        // Refactoring on this method is done (none needed)
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

        // Refactoring on this method is done
        private static void DeleteStreamWithUndo(ChemProV.PFD.Streams.StreamControl stream,
            DrawingCanvas canvas)
        {
            AbstractStream s = stream.Stream;
            
            // Special case: the old version (before I rewrote a bunch of logic) didn't let 
            // you delete heat streams whose destination was a heat exchanger with utility.
            // I will keep this functionality.
            if (s is HeatStream && s.Destination is HeatExchangerWithUtility)
            {
                return;
            }

            // We need to build a list of undos
            List<IUndoRedoAction> actions = new List<IUndoRedoAction>();

            if (null != s.Source)
            {
                // If the stream has a non-null source then we need to detach it and 
                // make sure the undo would reattach it
                actions.Add(new Logic.Undos.AttachOutgoingStream(s.Source, s));
                actions.Add(new Logic.Undos.SetStreamSource(s, s.Source, null, s.SourceLocation));

                // Do the detachment so that the process unit (which is staying around) won't 
                // have an outgoing stream that's been deleted
                s.Source.DetachOutgoingStream(s);
                s.Source = null;
            }
            if (null != s.Destination)
            {
                // If the stream has a non-null destination then we need to detach it and 
                // make sure the undo would reattach it
                actions.Add(new Logic.Undos.AttachIncomingStream(s.Destination, s));
                actions.Add(new Logic.Undos.SetStreamDestination(s, s.Destination, null, s.DestinationLocation));

                // Do the detachment so that the process unit (which is staying around) won't 
                // have an incoming stream that's been deleted
                s.Destination.DetachIncomingStream(s);
                s.Destination = null;
            }

            // In all cases we will need to add the stream back to the workspace
            actions.Add(new Logic.Undos.AddToWorkspace(stream.Stream));

            // Delete the stream from the workspace. Event handlers will take care of updating the UI.
            canvas.GetWorkspace().RemoveStream(stream.Stream);

            // Add the undo that we built
            canvas.GetWorkspace().AddUndo(
                new UndoRedoCollection("Undo deletion of stream", actions.ToArray()));
        }
    }
}
