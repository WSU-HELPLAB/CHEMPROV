/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
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