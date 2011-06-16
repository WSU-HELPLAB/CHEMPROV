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
    public class StreamSourceIcon : StreamEnd
    {
        /// <summary>
        /// This the rectangle that is drawn at the start of every stream
        /// </summary>
        private Rectangle sourceIcon;

        public Rectangle SourceIcon
        {
            get { return sourceIcon; }
            set { sourceIcon = value; }
        }
        public StreamSourceIcon(IStream stream, Rectangle source)
        {
            this.Stream = stream;
            this.sourceIcon = source;
        }
    }
}
