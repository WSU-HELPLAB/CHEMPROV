/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;

namespace ChemProV.PFD
{
    /// <summary>
    /// An undo/redo action is an object that provides an undo or redo for a specific action. 
    /// Most such actions will be things like adding a new object to the workspace, in which 
    /// case the undo action would remove the object and the redo action (created only upon 
    /// invokation of the undo action) would add it back.
    /// Every undo/redo action must have the ability to construct its "opposite", which will 
    /// be returned from the "Execute" function. So again taking the example of an undo 
    /// object that removes an object from the workspace upon execution, the "Execute" 
    /// function should return an object that would add it back.
    /// </summary>
    public interface IUndoRedoAction
    {
        IUndoRedoAction Execute(UndoRedoExecutionParameters parameters);
    }

    /// <summary>
    /// While objects that implement the IUndoRedoAction interface are expected to be constructed 
    /// with most of the information that they need, relevant application data is also provided 
    /// to the undo/redo object at the time of execution. Such information is stored in an 
    /// UndoRedoExecutionParameters object.
    /// </summary>
    public class UndoRedoExecutionParameters
    {
        private UI.DrawingCanvas.DrawingCanvas m_canvas;

        public UndoRedoExecutionParameters(UI.DrawingCanvas.DrawingCanvas canvas)
        {
            m_canvas = canvas;
        }

        public UI.DrawingCanvas.DrawingCanvas DrawingCanvas
        {
            get
            {
                return m_canvas;
            }
        }
    }

    /// <summary>
    /// This class serves as a container for an ordered collection of IUndoRedoAction objects. 
    /// The undo system contains a single UndoRedoCollection object for each undo action, as 
    /// far as the user is concerned. In other words, when the user clicks an "Undo" button or 
    /// menu item, a single UndoRedoCollection should be popped off of the undo stack and 
    /// executed.
    /// Although many undo actions will consist of just a single IUndoRedoAction object, this 
    /// undo system is designed with the capability to have more than one for future scenarios 
    /// where having multiple items may be relevant.
    /// </summary>
    public class UndoRedoCollection
    {
        // Things NOT to add to this class:
        // Do NOT add the ability to add new items to the collection after it has been 
        // instantiated. If there's a scenario where to want to build up a list of 
        // IUndoRedoAction objects and you don't want to do it in a single statement, 
        // then use a list of some sort (System.Collections.Generic.List for example) 
        // to add all your actions and then create the undo/redo collection by using 
        // the .ToArray() method on that list.
        // This keeps the collection immutable which has greater potential to reduce 
        // coding errors.
        
        
        private List<IUndoRedoAction> m_items = new List<IUndoRedoAction>();

        /// <summary>
        /// The title of the undo action. This is what the user will see if ChemProV is ever 
        /// extended to provide a more detailed explanation for the undo menu item. At the 
        /// time of this writing, regardless of what the last action the user performed was, 
        /// the undo item in the menu simply says "Undo". In the future if we wanted a better 
        /// menu item that said something like "Undo addition of mixer processing unit" that 
        /// description will be stored in this title.
        /// </summary>
        private string m_title = "Undo";

        /// <summary>
        /// Constructs an UndoRedoCollection that consists one or more IUndoRedoAction items 
        /// and uses the specified title.
        /// </summary>
        /// <param name="title">Title of the undo that will appear in the menu. For example: 
        /// "Undo addition of mixer processing unit". This title can be whatever is desired 
        /// but it must start with the text "Undo" or "Redo" (as appropriate).</param>
        /// <param name="actions">Array of actions for this collection. If this is null or 
        /// empty then an exception will be thrown.</param>
        public UndoRedoCollection(string title, params IUndoRedoAction[] actions)
        {
            if (null == actions || 0 == actions.Length)
            {
                throw new ArgumentException(
                    "An UndoRedoCollection cannot be constructed with a null or " +
                    "empty array of actions");
            }

            if (!title.StartsWith("Undo") && !title.StartsWith("Redo"))
            {
                throw new ArgumentException(
                    "UndoRedoCollection objects must have titles that start with \"Undo\" " +
                    "or \"Redo\"");
            }

            m_title = title;
            m_items.AddRange(actions);
        }

        /// <summary>
        /// Executes all items in the collection and returns a collection that would reverse 
        /// this action. In other words, if this is an undo action, then it returns the 
        /// collection for the redo. If it is a redo action then it will return a collection 
        /// that undoes the redo.
        /// Undos are executed from the beginning of the collection in the same order as they 
        /// were in within the array that was passed to the constructor.
        /// </summary>
        public UndoRedoCollection Execute(UndoRedoExecutionParameters parameters)
        {
            // Stores the return values from IUndoRedoAction.Execute.
            // This will be used to create the UndoRedoCollection that gets returned from 
            // this method
            List<IUndoRedoAction> opposites = new List<IUndoRedoAction>();

            // Execute all items in the list, storing their return values in our list 
            // of opposites
            foreach (IUndoRedoAction undoRedo in m_items)
            {
                opposites.Add(undoRedo.Execute(parameters));
            }

            // Determine the new title by changing "Undo" to "Redo" or "Redo" to "Undo"
            string newTitle = m_title.StartsWith("Undo") ? m_title.Replace("Undo", "Redo") :
                m_title.Replace("Redo", "Undo");

            // Construct and return the new collection
            return new UndoRedoCollection(newTitle, opposites.ToArray());
        }

        public string Title
        {
            get
            {
                return m_title;
            }
        }
    }
}