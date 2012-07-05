/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Xml.Serialization;

namespace ChemProV.PFD.Streams.PropertiesWindow
{
    public delegate void TableDataEventHandler(object sender, TableDataChangedEventArgs e);

    public interface IPropertiesWindow : IPfdElement, IXmlSerializable, IComparable, Core.ICanvasElement
    {
        /// <summary>
        /// This occurs when a change has been commited and the data changed as a result
        /// </summary>
        event TableDataEventHandler TableDataChanged;

        /// <summary>
        /// This occurs when a change is in progress but before it has been commited
        /// </summary>
        event EventHandler TableDataChanging;

        /// <summary>
        /// Gets or sets the parent stream that the table is attached to
        /// </summary>
        ChemProV.PFD.Streams.StreamControl ParentStream
        {
            get;
            set;
        }
    }
}