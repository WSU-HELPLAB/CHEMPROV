using System;
using ChemProV.Logic.Equations;

namespace ChemProV.Logic.Undos
{
    /// <summary>
    /// Represents and undo/redo action that will move an equation "up" within the collection. In 
    /// this context, "up" means decreasing its index within the collection.
    /// </summary>
    public class MoveEquationUp : IUndoRedoAction
    {
        private int m_index;

        public MoveEquationUp(int indexOfEquationToMoveUp)
        {
            if (indexOfEquationToMoveUp <= 0)
            {
                throw new ArgumentException(
                    "Cannot create an undo that moves the first equation (index 0) up.");
            }
            
            m_index = indexOfEquationToMoveUp;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            EquationModel moveMeUp = sender.Equations[m_index];
            sender.Equations.Remove(moveMeUp);
            sender.Equations.Insert(m_index - 1, moveMeUp);

            // Return move down action
            return new MoveEquationDown(m_index - 1);
        }
    }
}
