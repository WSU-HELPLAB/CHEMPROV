using System;
using ChemProV.Logic.Equations;

namespace ChemProV.Logic.Undos
{
    /// <summary>
    /// Represents and undo/redo action that will move an equation "down" within the collection. 
    /// In this context, "down" means increasing its index within the collection.
    /// </summary>
    public class MoveEquationDown : IUndoRedoAction
    {
        private int m_index;

        public MoveEquationDown(int indexOfEquationToMoveDown)
        {
            m_index = indexOfEquationToMoveDown;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            // Make sure it's not the last item in the collection (nor is the index beyond the 
            // last item in the collection).
            if (m_index >= sender.Equations.Count - 1)
            {
                throw new InvalidOperationException(string.Format(
                    "Cannot move equation at index {0} down within a collection of {1} items.",
                    m_index, sender.Equations.Count));
            }

            EquationModel moveMeDown = sender.Equations[m_index];
            sender.Equations.Remove(moveMeDown);
            sender.Equations.Insert(m_index + 1, moveMeDown);

            // Return move up action
            return new MoveEquationUp(m_index + 1);
        }
    }
}
