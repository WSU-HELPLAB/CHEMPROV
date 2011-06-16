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
    public interface IPaletteItem : IComparable
    {
        /// <summary>
        /// Sets the locaiton of the palette object's icon.  For a list of
        /// built-in icons, see the ChemProV.Palette.Icons class
        /// </summary>
        String IconSource
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the palette object's descriptor text.  Will appear to the right of
        /// the palette's icon.
        /// </summary>
        String Description
        {
            get;
            set;
        }

        /// <summary>
        /// Sets whether or not the current palette item is selected or not
        /// </summary>
        bool Selected
        {
            get;
            set;
        }

        /// <summary>
        /// Used as a hack to store further information related to a palette item.  
        /// Implemented specifically to store the ProcessUnit type in the
        /// ProcessUnitPaletteItem class.
        /// </summary>
        object Data
        {
            get;
        }
    }
}
