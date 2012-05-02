/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Windows;
using System.Xml.Linq;
using ChemProV.UI.DrawingCanvas;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.StickyNote;

namespace ChemProV.PFD.Streams
{
    /// <summary>
    /// Process unit constants to make object creation easier
    /// </summary>
    public enum StreamType
    {
        Chemical,
        Heat,
        Generic
    };

    public class StreamFactory
    {
        /// <summary>
        /// Translates a StreamType into an image.  Useful for generating
        /// images and such.
        /// </summary>
        /// <param name="unitType"></param>
        /// <returns></returns>
        public static string IconFromStreamType(StreamType unitType)
        {
            //note that I am not using "break" statements after each CASE
            //statement not because I'm lazy, but because VS2008 throws a
            //warning if I do (unreachable code)
            switch (unitType)
            {
                case StreamType.Chemical:
                    return "/UI/Icons/pu_stream.png";
                case StreamType.Heat:
                    return "/UI/Icons/pu_heat_stream.png";
                default:
                    return "/UI/Icons/pu_stream.png";
            }
        }

        /// <summary>
        /// Returns the stream type of the supplied object
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static StreamType StreamTypeFromStream(IStream stream)
        {
            if (stream is ChemicalStream)
            {
                return StreamType.Chemical;
            }
            else if (stream is HeatStream)
            {
                return StreamType.Heat;
            }
            else
            {
                return StreamType.Chemical;
            }
        }

        /// <summary>
        /// I've created this method
        /// that essentially will create a clone of the supplied stream.
        /// This performs a deep copy.
        /// </summary>
        /// <param name="stream">The stream to "clone"</param>
        /// <returns></returns>
        public static IStream StreamFromStreamObject(IStream stream)
        {
            IStream newStream = null;
            if (stream is ChemicalStream)
            {
                newStream = StreamFromStreamType(StreamType.Chemical);
                newStream.Destination = stream.Destination;
                newStream.Source = stream.Source;
            }
            else if (stream is HeatStream)
            {
                newStream = StreamFromStreamType(StreamType.Heat);
                newStream.Destination = stream.Destination;
                newStream.Source = stream.Source;
            }

            return newStream;
        }

        /// <summary>
        /// Creates a new stream based on the supplied stream type
        /// </summary>
        /// <param name="unitType"></param>
        /// <returns></returns>
        public static IStream StreamFromStreamType(StreamType unitType)
        {
            if (StreamType.Chemical == unitType)
            {
                return new ChemicalStream();
            }
            else if (StreamType.Heat == unitType)
            {
                return new HeatStream();
            }
            else
                return new ChemicalStream();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public static IStream StreamFromXml(XElement element, DrawingCanvas owner, bool attachProcessUnits)
        {
            // Get the ID attribute
            string id = (string)element.Attribute("Id");

            // Get the process unit type
            string unitType = (string)element.Attribute("StreamType");

            IStream stream = null;
            if ("Chemical" == unitType)
            {
                stream = new ChemicalStream(owner);
            }
            else
            {
                // Right now ChemProV only has chemical and heat streams
                stream = new HeatStream(owner);
            }

            // E.O.
            // Check for unconnected endpoints
            XElement usEl = element.Element("UnattachedSource");
            if (null != usEl)
            {
                XAttribute locAttr = usEl.Attribute("Location");
                if (null != locAttr)
                {
                    Point loc;
                    if (Core.App.TryParsePoint(locAttr.Value, out loc))
                    {
                        (stream as AbstractStream).SourceDragIcon.Location = loc;
                    }
                }
            }
            XElement udEl = element.Element("UnattachedDestination");
            if (null != udEl)
            {
                XAttribute locAttr = udEl.Attribute("Location");
                if (null != locAttr)
                {
                    Point loc;
                    if (Core.App.TryParsePoint(locAttr.Value, out loc))
                    {
                        (stream as AbstractStream).DestinationDragIcon.Location = loc;
                    }
                }
            }

            // Read source and destination process unit IDs (if present)
            string srcID = null, dstID = null;
            XElement srcEl = element.Element("Source");
            if (null != srcEl)
            {
                srcID = srcEl.Value;
            }
            XElement dstEl = element.Element("Destination");
            if (null != dstEl)
            {
                dstID = dstEl.Value;
            }

            // If attachProcessUnits is true then it's assumed that the drawing canvas has all process 
            // units already added to it, so we can make connections
            if (attachProcessUnits)
            {
                foreach (UIElement uie in owner.Children)
                {
                    if (!(uie is GenericProcessUnit))
                    {
                        continue;
                    }

                    GenericProcessUnit gpu = uie as GenericProcessUnit;

                    // See if this process unit is either the source or destination for the stream
                    if (null != srcID && gpu.Id.Equals(srcID))
                    {
                        // Connect as source
                        stream.Source = gpu;
                        gpu.AttachOutgoingStream(stream);
                    }
                    if (null != dstID && gpu.Id.Equals(dstID))
                    {
                        // Connect as destination
                        stream.Destination = gpu;
                        gpu.AttachIncomingStream(stream);
                    }
                }
            }

            // Show the drag icons
            (stream as AbstractStream).SourceDragIcon.Visibility = Visibility.Visible;
            (stream as AbstractStream).DestinationDragIcon.Visibility = Visibility.Visible;

            // Load any comments that are present
            XElement cmtElement = element.Element("Comments");
            if (null != cmtElement)
            {
                foreach (XElement child in cmtElement.Elements())
                {
                    PFD.StickyNote.StickyNote sn = PFD.StickyNote.StickyNote.CreateCommentNote(
                        owner, (stream as Core.ICommentCollection), child);

                    (stream as AbstractStream).AddComment(sn);
                }
            }

            stream.Id = id;
            return stream;
        }
    }
}