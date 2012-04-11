/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using ChemProV.PFD.Streams;

namespace ChemProV.PFD.ProcessUnits
{
    /// <summary>
    /// Process unit constants to make object creation easier
    /// </summary>
    public enum ProcessUnitType
    {
        Blank,
        Generic,
        HeatExchanger,
        HeatExchangerNoUtility,
        Mixer,
        Separator,
        Reactor,
        Sink,
        Source
    };

    /// <summary>
    /// Concrete implementation of the IProcessUnitFactory interface
    /// </summary>
    public class ProcessUnitFactory
    {
        /// <summary>
        /// Turns a string pointing to an image into an Image object
        /// </summary>
        /// <param name="uri">The location of the image</param>
        /// <returns>A fancy new Image object</returns>
        private static Image ImageFromString(String uri)
        {
            BitmapImage bmp = new BitmapImage();
            bmp.UriSource = new Uri(uri, UriKind.Relative);
            Image img = new Image();
            img.SetValue(Image.SourceProperty, bmp);
            return img;
        }

        /// <summary>
        /// Translates a ProcessUnitType into an image.  Useful for generating
        /// images and such.
        /// </summary>
        /// <param name="unitType"></param>
        /// <returns></returns>
        public static string IconFromUnitType(ProcessUnitType unitType)
        {
            //note that I am not using "break" statements after each CASE
            //statement not because I'm lazy, but because VS2008 throws a
            //warning if I do (unreachable code)
            switch (unitType)
            {
                case ProcessUnitType.Blank:
                    return "/UI/Icons/pu_blank.png";

                case ProcessUnitType.Generic:
                    return "/UI/Icons/pu_generic.png";

                case ProcessUnitType.HeatExchanger:
                    return "/UI/Icons/pu_heat_exchanger.png";

                case ProcessUnitType.HeatExchangerNoUtility:
                    return "/UI/Icons/pu_heat_exchanger_no_utility.png";

                case ProcessUnitType.Mixer:
                    return "/UI/Icons/pu_mixer.png";

                case ProcessUnitType.Separator:
                    return "/UI/Icons/pu_separator.png";

                case ProcessUnitType.Sink:
                    return "/UI/Icons/pu_sink.png";

                case ProcessUnitType.Source:
                    return "/UI/Icons/pu_source.png";

                case ProcessUnitType.Reactor:
                    return "/UI/Icons/pu_reactor.png";

                default:
                    return "/UI/Icons/pu_generic.png";
            }
        }

        /// <summary>
        /// Since we're not currently keeping track of process unit types, we need this function
        /// to tell us the type of a given process unit.
        /// </summary>
        /// <param name="unit">The Process Unit to test</param>
        /// <returns></returns>
        public static ProcessUnitType GetProcessUnitType(IProcessUnit unit)
        {
            if (unit.Description.CompareTo(ProcessUnitDescriptions.Blank) == 0)
            {
                return ProcessUnitType.Blank;
            }
            else if (unit.Description.CompareTo(ProcessUnitDescriptions.Generic) == 0)
            {
                return ProcessUnitType.Generic;
            }
            else if (unit.Description.CompareTo(ProcessUnitDescriptions.HeatExchanger) == 0)
            {
                return ProcessUnitType.HeatExchanger;
            }
            else if (unit.Description.CompareTo(ProcessUnitDescriptions.HeatExchangerNoUtility) == 0)
            {
                return ProcessUnitType.HeatExchangerNoUtility;
            }
            else if (unit.Description.CompareTo(ProcessUnitDescriptions.Mixer) == 0)
            {
                return ProcessUnitType.Mixer;
            }
            else if (unit.Description.CompareTo(ProcessUnitDescriptions.Separator) == 0)
            {
                return ProcessUnitType.Separator;
            }
            else if (unit.Description.CompareTo(ProcessUnitDescriptions.Reactor) == 0)
            {
                return ProcessUnitType.Reactor;
            }
            else if (unit.Description.CompareTo(ProcessUnitDescriptions.Sink) == 0)
            {
                return ProcessUnitType.Sink;
            }
            else if (unit.Description.CompareTo(ProcessUnitDescriptions.Source) == 0)
            {
                return ProcessUnitType.Source;
            }
            return ProcessUnitType.Generic;
        }

        /// <summary>
        /// Not sure if this is the best way to go about this, but since SilverLight
        /// doesnt support the ICloneable interface (wtf?), I've created this method
        /// that essentially will create a clone of the supplied process unit.
        /// Currently, this performs a shallow copy.
        /// </summary>
        /// <param name="unit">The process unit to "clone"</param>
        /// <returns></returns>
        public static IProcessUnit ProcessUnitFromProcessUnit(IProcessUnit unit)
        {
            //call the factory method to create a generic process unit
            IProcessUnit pu = ProcessUnitFromUnitType(GetProcessUnitType(unit));

            //copy over the incoming streams
            foreach (IStream stream in unit.IncomingStreams)
            {
                pu.IncomingStreams.Add(stream);
            }

            //and the outgoing streams
            foreach (IStream stream in unit.OutgoingStreams)
            {
                pu.OutgoingStreams.Add(stream);
            }

            //return the shallow-copied process unit
            return pu;
        }

        /// <summary>
        /// Creates a new Process Unit (derived from IProcessUnit) based on the
        /// supplied ProcessUnitTypes
        /// </summary>
        /// <param name="unitType">Specifies the type of unit to create</param>
        /// <returns>The new process unit</returns>
        public static IProcessUnit ProcessUnitFromUnitType(string unitType)
        {
            //turn the string into an enum and return the process unit
            ProcessUnitType type = (ProcessUnitType)Enum.Parse(typeof(ProcessUnitType), unitType, true);
            return ProcessUnitFromUnitType(type);
        }

        /// <summary>
        /// Creates a new Process Unit (derived from IProcessUnit) based on the
        /// supplied ProcessUnitTypes
        /// </summary>
        /// <param name="unitType">Specifies the type of unit to create</param>
        /// <returns>The new process unit</returns>
        public static IProcessUnit ProcessUnitFromUnitType(ProcessUnitType unitType)
        {
            //Note that I'm not assigning an object to pu.  If you examine the
            //case statement below, it's apparent (at the time of writing
            //this note) that I could just create a GenericProcessUnit.  However
            //in the future, we may need to change this, so I'm just thinking of that.
            IProcessUnit pu;

            //MaxIncomingSteam / MaxOutGoingSteams: -1 is infinity
            //Will need to fix Engery types MaxIn/Out Streams and will need to add energy stream type.
            switch (unitType)
            {
                case ProcessUnitType.Blank:
                    pu = new TemporaryProcessUnit();
                    pu.MaxIncomingStreams = 1;
                    pu.MaxOutgoingStreams = 1;
                    pu.MaxIncomingHeatStreams = 1;
                    pu.MaxOutgoingHeatStreams = 1;
                    pu.Description = ProcessUnitDescriptions.Blank;
                    break;

                case ProcessUnitType.Generic:
                    pu = new GenericProcessUnit();
                    pu.MaxIncomingStreams = 1;
                    pu.MaxOutgoingStreams = 1;
                    pu.MaxIncomingHeatStreams = 1;
                    pu.MaxOutgoingHeatStreams = 1;
                    pu.Description = ProcessUnitDescriptions.Generic;
                    break;

                case ProcessUnitType.HeatExchanger:
                    pu = new LabeledProcessUnit();
                    (pu as LabeledProcessUnit).ProcessUnitLabel = "Exc" + pu.ProcessUnitId;
                    pu.MaxIncomingStreams = 1;
                    pu.MaxOutgoingStreams = 1;
                    pu.MaxIncomingHeatStreams = 1;
                    pu.MaxOutgoingHeatStreams = 0;
                    pu.Description = ProcessUnitDescriptions.HeatExchanger;
                    break;

                case ProcessUnitType.HeatExchangerNoUtility:
                    pu = new LabeledProcessUnit();
                    (pu as LabeledProcessUnit).ProcessUnitLabel = "Exc" + pu.ProcessUnitId;
                    pu.MaxIncomingStreams = 2;
                    pu.MaxOutgoingStreams = 2;
                    pu.MaxIncomingHeatStreams = 0;
                    pu.MaxOutgoingHeatStreams = 0;
                    pu.Description = ProcessUnitDescriptions.HeatExchangerNoUtility;
                    break;

                case ProcessUnitType.Mixer:
                    pu = new LabeledProcessUnit();
                    (pu as LabeledProcessUnit).ProcessUnitLabel = "Mix" + pu.ProcessUnitId;
                    pu.MaxIncomingStreams = -1;
                    pu.MaxOutgoingStreams = 1;
                    pu.MaxIncomingHeatStreams = 0;
                    pu.MaxOutgoingHeatStreams = 0;
                    pu.Description = ProcessUnitDescriptions.Mixer;
                    break;

                case ProcessUnitType.Reactor:
                    pu = new LabeledProcessUnit();
                    (pu as LabeledProcessUnit).ProcessUnitLabel = "Rct" + pu.ProcessUnitId;
                    pu.MaxIncomingStreams = -1;
                    pu.MaxOutgoingStreams = 1;
                    pu.MaxIncomingHeatStreams = 1;
                    pu.MaxOutgoingHeatStreams = 0;
                    pu.Description = ProcessUnitDescriptions.Reactor;
                    break;

                case ProcessUnitType.Separator:
                    pu = new LabeledProcessUnit();
                    (pu as LabeledProcessUnit).ProcessUnitLabel = "Sep" + pu.ProcessUnitId;
                    pu.MaxIncomingStreams = 1;
                    pu.MaxOutgoingStreams = -1;
                    pu.MaxIncomingHeatStreams = 0;
                    pu.MaxOutgoingHeatStreams = 0;
                    pu.Description = ProcessUnitDescriptions.Separator;
                    break;

                case ProcessUnitType.Sink:
                    pu = new TemporaryProcessUnit();
                    pu.MaxIncomingStreams = 1;
                    pu.MaxOutgoingStreams = 0;
                    pu.MaxIncomingHeatStreams = 1;
                    pu.MaxOutgoingHeatStreams = 0;
                    pu.Description = ProcessUnitDescriptions.Sink;
                    ((GenericProcessUnit)pu).Height = 20;
                    ((GenericProcessUnit)pu).Width = 20;
                    break;

                case ProcessUnitType.Source:
                    pu = new TemporaryProcessUnit();
                    pu.MaxIncomingStreams = 0;
                    pu.MaxOutgoingStreams = 1;
                    pu.MaxIncomingHeatStreams = 0;
                    pu.MaxOutgoingHeatStreams = 1;
                    pu.Description = ProcessUnitDescriptions.Source;
                    ((GenericProcessUnit)pu).Height = 20;
                    ((GenericProcessUnit)pu).Width = 20;
                    break;

                default:
                    pu = new GenericProcessUnit();
                    pu.MaxIncomingStreams = 1;
                    pu.MaxOutgoingStreams = 1;
                    pu.MaxIncomingHeatStreams = 1;
                    pu.MaxOutgoingHeatStreams = 1;
                    pu.Description = ProcessUnitDescriptions.Generic;
                    break;
            }

            pu.Icon = ImageFromString(IconFromUnitType(unitType));
            return pu;
        }

        public static IProcessUnit ProcessUnitFromXml(XElement element)
        {
            //pull the process unit type
            string unitType = (string)element.Attribute("ProcessUnitType");

            //call the factory to create a new object for us
            IProcessUnit pu = ProcessUnitFromUnitType(unitType);

            //hand the heavy lifting off to the individual process unit
            pu = pu.FromXml(element, pu);
            return pu;
        }
    }
}