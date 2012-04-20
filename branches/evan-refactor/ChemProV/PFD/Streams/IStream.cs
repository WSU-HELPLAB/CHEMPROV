/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Windows.Input;
using System.Xml.Serialization;

using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams.PropertiesWindow;

namespace ChemProV.PFD.Streams
{
    public interface IStream : IPfdElement, IXmlSerializable
    {
        /// <summary>
        /// Reference to the stream's source PFD element
        /// </summary>
        IProcessUnit Source
        {
            get;
            set;
        }

        /// <summary>
        /// Reference to the stream's destination PFD element
        /// </summary>
        IProcessUnit Destination
        {
            get;
            set;
        }

        /// <summary>
        /// Reference to the stream's table PFD element
        /// </summary>
        IPropertiesWindow Table
        {
            get;
            set;
        }

        /// <summary>
        /// this holds the stream and the polygon for the arrow.
        /// </summary>
        StreamDestinationIcon StreamDestination
        {
            get;
            set;
        }

        /// <summary>
        /// this holds the stream and the rectangle for the rectangle at the beginning of the stream
        /// </summary>
        StreamSourceIcon StreamSource
        {
            get;
            set;
        }

        /// <summary>
        /// Can be called to manually update the stream's location
        /// </summary>
        void UpdateStreamLocation();

        event MouseButtonEventHandler Arrow_MouseButtonLeftDown;
        event MouseButtonEventHandler Tail_MouseButtonLeftDown;

        /// <summary>
        /// E.O.
        /// Returns a boolean value indicating whether or not this stream should be available 
        /// with the specified difficulty setting.
        /// </summary>
        /// <returns>True if available with the difficulty setting, false otherwise.</returns>
        bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty);

        /// <summary>
        /// E.O.
        /// Every stream has a source and destination process unit. This method determines whether or 
        /// not the specified unit is a valid source. This may depend on multiple things, such 
        /// as the type of the stream and whether or not the process unit can accept more outgoing 
        /// streams.
        /// </summary>
        /// <returns>True if the unit is a valid source, false otherwise.</returns>
        bool IsValidSource(IProcessUnit unit);

        /// <summary>
        /// E.O.
        /// Every stream has a source and destination process unit. This method determines whether or 
        /// not the specified unit is a valid destination. This may depend on multiple things, such 
        /// as the type of the stream and whether or not the process unit can accept more incoming 
        /// streams.
        /// </summary>
        /// <returns>True if the unit is a valid destination, false otherwise.</returns>
        bool IsValidDestination(IProcessUnit unit);

        /// <summary>
        /// E.O.
        /// The name for the stream that will appear in the user interface
        /// </summary>
        string Title
        {
            get;
        }
    }
}