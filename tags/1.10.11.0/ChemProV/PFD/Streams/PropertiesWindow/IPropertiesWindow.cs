/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Xml.Serialization;

using ChemProV.PFD;

namespace ChemProV.PFD.Streams.PropertiesWindow
{
    public delegate void TableDataEventHandler(object sender, TableDataChangedEventArgs e);

    public interface IPropertiesWindow : IPfdElement, IXmlSerializable, IComparable
    {
        event TableDataEventHandler TableDataChanged; 


        /// <summary>
        /// Gets or sets the parent stream that the table is attached to
        /// </summary>
        IStream ParentStream
        {
            get;
            set;
        }


    }
}
