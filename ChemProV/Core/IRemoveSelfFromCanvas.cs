using System;

namespace ChemProV.Core
{
    public interface IRemoveSelfFromCanvas
    {
        /// <summary>
        /// Tells the control to remove itself, and any relevant child controls it owns, from the 
        /// DrawingCanvas. This is considered disposal of the control and it should not keep any 
        /// object references or monitor any events after removal.
        /// </summary>
        void RemoveSelfFromCanvas(ChemProV.UI.DrawingCanvas owner);
    }
}