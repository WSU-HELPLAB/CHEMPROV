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
    public class StreamDestinationIcon : StreamEnd
    {
        /// <summary>
        /// This the polygon that is drawn on the end of stream
        /// </summary>
        private Polygon destinationIcon;

        public Polygon DestinationIcon
        {
            get { return destinationIcon; }
            set { destinationIcon = value; }
        }

        public StreamDestinationIcon(IStream stream, Polygon destination)
        {
            this.Stream = stream;
            this.destinationIcon = destination;
        }
    }
}
