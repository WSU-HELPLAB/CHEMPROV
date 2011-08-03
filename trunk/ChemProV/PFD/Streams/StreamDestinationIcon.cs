/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL) v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

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