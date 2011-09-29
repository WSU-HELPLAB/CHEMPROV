/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/
using System.Collections.Generic;

/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System.Windows;
using System.Windows.Controls;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.UI.DrawingCanvas.Commands;

namespace ChemProV.UI.DrawingCanvas
{
    public class ProcessUnitUndo : SavedStateObject
    {
        private CanvasCommands commandIssed;

        public CanvasCommands CommandIssed
        {
            get { return commandIssed; }
        }

        private IProcessUnit iPUManipulated;

        public IProcessUnit IPUManipulated
        {
            get { return iPUManipulated; }
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
        /// This is for when the ProcessUnit is deleted and incase it takes streams with it then we need to save those guys so we can retore them.
        /// </summary>
        private Stack<StreamUndo> connectedStream = new Stack<StreamUndo>();

        public Stack<StreamUndo> ConnectedStream
        {
            get { return connectedStream; }
        }

        /// <summary>
        /// This sets the need infomation inorder to get the state of the process unit back.
        /// </summary>
        /// <param name="commandIssued">The command that was issued and the reason we are saving it</param>
        /// <param name="ProcessUnitManipulated">Reference to the process unit that the command is on</param>
        /// <param name="CanvasUsed">Reference to the drawing_canvas we are using</param>
        /// <param name="location">The top left location of the Process, unit before it has been moved / deleted, not needed if process is being added</param>
        public ProcessUnitUndo(CanvasCommands commandIssued, IProcessUnit ProcessUnitManipulated, Canvas CanvasUsed, Point location)
        {
            this.commandIssed = commandIssued;
            this.iPUManipulated = ProcessUnitManipulated;
            this.theCanvasUsed = CanvasUsed;

            //need to find middle point since that is what the commands use
            this.location = new Point(location.X, location.Y);

            //so we are about to remove an IProcessUnit from the drawing_canvas we gotta save his streams too.
            if (commandIssed == CanvasCommands.RemoveFromCanvas && !(ProcessUnitManipulated is TemporaryProcessUnit))
            {
                foreach (IStream stream in iPUManipulated.IncomingStreams)
                {
                    connectedStream.Push(new StreamUndo(commandIssued, stream, CanvasUsed, stream.Source as IProcessUnit, stream.Destination as IProcessUnit, new Point()));
                }
                foreach (IStream stream in iPUManipulated.OutgoingStreams)
                {
                    connectedStream.Push(new StreamUndo(commandIssued, stream, CanvasUsed, stream.Source as IProcessUnit, stream.Destination as IProcessUnit, new Point()));
                }
            }
        }
    }
}