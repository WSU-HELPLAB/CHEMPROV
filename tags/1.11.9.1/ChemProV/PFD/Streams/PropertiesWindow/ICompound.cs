/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System.Collections.Generic;

namespace ChemProV.PFD.Streams.PropertiesWindow
{
    public interface ICompound
    {
        List<KeyValuePair<Element, int>> elements
        {
            get;
        }
    }
}