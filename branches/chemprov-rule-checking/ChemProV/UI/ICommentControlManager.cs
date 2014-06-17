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

namespace ChemProV.UI
{
    /// <summary>
    /// Represents a control that manages a collection of child sticky note controls 
    /// representing comments.
    /// </summary>
    public interface ICommentControlManager
    {
        void HideAllComments();
        
        void ShowAllComments();
    }
}
