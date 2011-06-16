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
    /// This is what all commands inherriet from
    /// </summary>
    public interface ICommand
    {
        bool Execute();
    }
}
