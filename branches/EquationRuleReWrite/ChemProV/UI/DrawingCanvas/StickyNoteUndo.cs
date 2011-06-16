/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System.Windows;
using System.Windows.Controls;
using ChemProV.PFD.StickyNote;
using ChemProV.UI.DrawingCanvas.Commands;

namespace ChemProV.UI.DrawingCanvas
{
    /// <summary>
    /// This is used to save the state of the StickyNote so it can be undone later
    /// </summary>
    public class StickyNoteUndo : SavedStateObject
    {
        private CanvasCommands commandIssed;

        /// <summary>
        /// This is how we are changing it
        /// </summary>
        public CanvasCommands CommandIssed
        {
            get { return commandIssed; }
        }

        private StickyNote snManipulated;

        /// <summary>
        /// This is the StickyNote that we are changing.
        /// </summary>
        public StickyNote SnManipulated
        {
            get { return snManipulated; }
        }

        private Canvas theCanvasUsed;

        /// <summary>
        /// This is the drawing_canvas that used when the command was executed
        /// </summary>
        public Canvas TheCanvasUsed
        {
            get { return theCanvasUsed; }
        }

        private Point location;

        /// <summary>
        /// This is the location it was at.
        /// </summary>
        public Point Location
        {
            get { return location; }
        }

        /// <summary>
        /// This sets the needed information inorder to get the state of the stickynote back.
        /// </summary>
        /// <param name="commandIssued">The command that was issued and the reason we are saving it</param>
        /// <param name="snManipulated">Reference to the stickyNote that the command is on</param>
        /// <param name="CanvasUsed">Reference to the drawing_canvas we are using</param>
        /// <param name="location">The top left location of the Process, unit before it has been moved / deleted, not needed if process is being added</param>
        public StickyNoteUndo(CanvasCommands commandIssued, StickyNote snManipulated, Canvas CanvasUsed, Point location)
        {
            this.commandIssed = commandIssued;
            this.snManipulated = snManipulated;
            this.theCanvasUsed = CanvasUsed;
            //need to find middle point since that is what the commands use
            this.location = new Point(location.X + (snManipulated as UserControl).ActualWidth / 2, location.Y);
        }
    }
}