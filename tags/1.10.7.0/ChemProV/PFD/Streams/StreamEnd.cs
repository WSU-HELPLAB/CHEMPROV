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

namespace ChemProV.PFD.Streams
{
    /// <summary>
    /// This should be an interface but dunno how exactly...
    /// </summary>
    public abstract class StreamEnd : IPfdElement
    {

        public event EventHandler LocationChanged;

        public bool Selected
        {
            get
            {
                return stream.Selected;
            }
            set
            {
                stream.Selected = value;
            }
        }

        public event EventHandler SelectionChanged;

        /// <summary>
        /// This the IStrem that is connected to the end.
        /// </summary>
        protected IStream stream;

        public IStream Stream
        {
            get { return stream; }
            set { stream = value; }
        }


        public StreamEnd()
        {
            
        }

        #region IPfdElement Members


        /// <summary>
        /// StreamEnds don't really need an Id, so always return 0.
        /// </summary>
        public string Id
        {
            get
            {
                return "0";
            }
            set
            {
                
            }
        }

        #endregion
    }
}
