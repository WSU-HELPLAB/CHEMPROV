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


namespace ChemProV.UI.DrawingCanvas.States
{
    /// <summary>
    /// This is the base class that all states inherite from.
    /// </summary>
    public interface IState
    {
        void MouseEnter(object sender, MouseEventArgs e);
        void MouseLeave(object sender, MouseEventArgs e);
        void MouseMove(object sender, MouseEventArgs e);
        void MouseLeftButtonDown(object sender, MouseButtonEventArgs e);
        void MouseLeftButtonUp(object sender, MouseButtonEventArgs e);
        void MouseRightButtonDown(object sender, MouseButtonEventArgs e);
        void MouseRightButtonUp(object sender, MouseButtonEventArgs e);
        void MouseWheel(object sender, MouseEventArgs e);
    }
}
