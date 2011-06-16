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
