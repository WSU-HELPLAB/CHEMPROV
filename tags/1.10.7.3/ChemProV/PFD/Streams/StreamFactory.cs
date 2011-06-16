using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

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
        public static StreamType StreamTypeFromObject(IStream stream)
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
        /// Creates a new stream based on the supplied stream type
        /// </summary>
        /// <param name="unitType"></param>
        /// <returns></returns>
        public static IStream StreamFromStreamType(string unitType)
        {
            //turn the string into an enum and return the stream
            StreamType type = (StreamType)Enum.Parse(typeof(StreamType), unitType, true);
            return StreamFromStreamType(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IStream StreamFromXml(XElement element)
        {
            //pull the attribute
            string id = (string)element.Attribute("Id");

            //pull the process unit type
            string unitType = (string)element.Attribute("StreamType");

            //call the factory to create a new object for us
            IStream stream = StreamFromStreamType(unitType);
            stream.Id = id;
            return stream;            
        }
    }
}
