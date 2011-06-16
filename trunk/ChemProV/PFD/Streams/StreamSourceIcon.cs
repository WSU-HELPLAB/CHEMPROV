/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

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