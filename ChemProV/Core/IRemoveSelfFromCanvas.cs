using System;

namespace ChemProV.Core
{
    public interface IRemoveSelfFromCanvas
    {
        /// <summary>
        /// Tells the control to remove itself, and any relevant child controls it owns, from the 
        /// DrawingCanvas.
        /// </summary>
        void RemoveSelfFromCanvas(ChemProV.UI.DrawingCanvas.DrawingCanvas owner);
    }
}