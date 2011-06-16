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

using ChemProV.UI.DrawingCanvas.Commands;
using ChemProV.PFD.Streams;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD;
using ChemProV.PFD.StickyNote;
using ChemProV;


namespace ChemProV.UI.DrawingCanvas
{
    public class StickyNoteUndo : SavedStateObject
    {
        private CanvasCommands commandIssed;

        public CanvasCommands CommandIssed
        {
            get { return commandIssed; }
        }

        private StickyNote snManipulated;

        public StickyNote SnManipulated
        {
            get { return snManipulated; }
        }

        private Canvas theCanvasUsed;

        public Canvas TheCanvasUsed
        {
            get { return theCanvasUsed; }
        }

        private Point location;

        public Point Location
        {
            get { return location; }
        }

        /// <summary>
        /// This sets the needed information inorder to get the state of the stickynote back.
        /// </summary>
        /// <param name="commandIssued">The command that was issued and the reason we are saving it</param>
        /// <param name="ProcessUnitManipulated">Reference to the stickyNote that the command is on</param>
        /// <param name="CanvasUsed">Reference to the canvas we are using</param>
        /// <param name="newLocation">The top left location of the Process, unit before it has been moved / deleted, not needed if process is being added</param>
        public StickyNoteUndo(CanvasCommands commandIssued, StickyNote snManipulated, Canvas CanvasUsed, Point location)
        {
            this.commandIssed = commandIssued;
            this.snManipulated = snManipulated;
            this.theCanvasUsed = CanvasUsed;
            //need to find middle point since that is what the commands use
            this.location = new Point(location.X + (snManipulated as UserControl).ActualWidth / 2, location.Y + (snManipulated as UserControl).ActualHeight / 2);
        }
    }
}