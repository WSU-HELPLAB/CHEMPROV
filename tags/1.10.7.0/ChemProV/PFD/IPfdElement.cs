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

namespace ChemProV.PFD
{
    public interface IPfdElement
    {
        /// <summary>
        /// Fired when the PFD element's location is changed
        /// </summary>
        event EventHandler LocationChanged;

        /// <summary>
        /// Gets or sets the IProcessUnit's unique ID number
        /// </summary>
        String Id
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether or not the IPfdElement has been selected.
        /// </summary>
        Boolean Selected
        {
            get;
            set;
        }

        /// <summary>
        /// Fired whenever the stream's selection status changes
        /// </summary>
        event EventHandler SelectionChanged;
    }
}
