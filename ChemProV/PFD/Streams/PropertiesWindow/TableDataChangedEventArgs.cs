/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System;

namespace ChemProV.PFD.Streams.PropertiesWindow
{
    public class TableDataChangedEventArgs : EventArgs
    {
        public readonly object data;
        public readonly string propertyName;

        public TableDataChangedEventArgs(object data, string propertyName)
        {
            this.data = data;
            this.propertyName = propertyName;
        }

        public TableDataChangedEventArgs()
        {
            this.data = null;
            this.propertyName = "";
        }
    }
}