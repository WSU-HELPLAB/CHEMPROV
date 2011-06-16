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
    public class DrawingCanvasCommandEventArgs
    {
        public readonly Point location;

        public DrawingCanvasCommandEventArgs(Point location = new Point())
        {
            this.location = location;
        }
    }
}
