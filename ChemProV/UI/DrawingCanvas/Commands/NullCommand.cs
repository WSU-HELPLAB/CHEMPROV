/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

namespace ChemProV.UI.DrawingCanvas.Commands
{
    /// <summary>
    /// Our default command.
    /// </summary>
    public class NullCommand : ICommand
    {
        private static ICommand instance;

        /// <summary>
        /// Used to get at the single instance of this object
        /// </summary>
        /// <returns></returns>
        public static ICommand GetInstance()
        {
            if (instance == null)
            {
                instance = new NullCommand();
            }
            return instance;
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        private NullCommand()
        {
        }

        /// <summary>
        /// Empty execute funciton
        /// </summary>
        public bool Execute()
        {
            return true;
        }
    }
}