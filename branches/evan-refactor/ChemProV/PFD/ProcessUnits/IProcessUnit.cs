/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using ChemProV.PFD.Streams;
using System.Xml.Linq;
using ChemProV.Core;

namespace ChemProV.PFD.ProcessUnits
{
    /// <summary>
    /// Interface that must be implemented by any process unit.  This outlines all of the basic
    /// functionality encapsulated by any process unit.
    /// </summary>
    public interface IProcessUnit : IPfdElement, IXmlSerializable, ICanvasElement
    {
        event EventHandler StreamsChanged;

        /// <summary>
        /// Shorthand for getting the IProcessUnit's integer component of its id
        /// </summary>
        int ProcessUnitId
        {
            get;
        }

        /// <summary>
        /// All process units need an icon so they can be represented in a drawing drawing_canvas
        /// </summary>
        Image Icon
        {
            get;
        }

        /// <summary>
        /// E.O.
        /// Returns a boolean value indicating whether or not this processing unit should be available 
        /// with the specified difficulty setting.
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns>True if available with the difficulty setting, false otherwise.</returns>
        bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty);

        /// <summary>
        /// A short description of the process unit.  Not more than a few words in length.
        /// </summary>
        String Description
        {
            get;
        }

        /// <summary>
        /// Total number of incoming streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        int MaxIncomingStreams
        {
            get;
        }

        /// <summary>
        /// Total number of outgoing streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        int MaxOutgoingStreams
        {
            get;
        }

        /// <summary>
        /// Total number of incoming heat streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        int MaxIncomingHeatStreams
        {
            get;
        }

        /// <summary>
        /// Total number of outgoing heat streams allowed.
        /// A value of -1 means unlimited.
        /// </summary>
        int MaxOutgoingHeatStreams
        {
            get;
        }

        Point MidPoint
        {
            get;

            // E.O.
            set;
        }

        /// <summary>
        /// Gets whether or not the IProcessUnit is accepting new incoming streams
        /// </summary>
        bool IsAcceptingIncomingStreams(IStream stream);

        /// <summary>
        /// Gets whether or not the IProcessUnit is accepting new outgoing streams
        /// </summary>
        bool IsAcceptingOutgoingStreams(IStream stream);

        /// <summary>
        /// Attaches a new incoming stream to the IProcessUnit
        /// </summary>
        /// <param name="stream">The IStream to attach</param>
        /// <returns>Whether or not the stream was successfully attached</returns>
        bool AttachIncomingStream(IStream stream);

        /// <summary>
        /// Attaches a new outgoing stream to the IProcessUnit
        /// </summary>
        /// <param name="stream">The IStream to attach</param>
        /// <returns>Whether or not the stream was successfully attached</returns>
        bool AttachOutgoingStream(IStream stream);

        /// <summary>
        /// Dettaches an incoming stream to the IProcessUnit
        /// </summary>
        /// <param name="stream">The IStream to dettach</param>
        void DettachIncomingStream(IStream stream);

        /// <summary>
        /// Dettaches an outgoing stream to the IProcessUnit
        /// </summary>
        /// <param name="stream">The IStream to dettach</param>
        void DettachOutgoingStream(IStream stream);

        /// <summary>
        /// List of incoming streams
        /// </summary>
        IList<IStream> IncomingStreams
        {
            get;
        }

        /// <summary>
        /// List of outgoing streams
        /// </summary>
        IList<IStream> OutgoingStreams
        {
            get;
        }

        /// <summary>
        /// All process units must support parsing from XML
        /// </summary>
        /// <param name="xpu"></param>
        /// <returns></returns>
        IProcessUnit FromXml(XElement xpu, IProcessUnit targetUnit);

        /// <summary>
        /// E.O.
        /// Not sure if defining this as a color is the best thing to do, but that's 
        /// essentially all it will be in the interface is a color. All colors should 
        /// be fully opaque (alpha=255). The color white RGBA(255, 255, 255, 255) will 
        /// be default.
        /// </summary>
        System.Windows.Media.Color Subprocess
        {
            get;
            set;
        }
    }
}