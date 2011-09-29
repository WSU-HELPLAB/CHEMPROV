/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

namespace ChemProV.PFD.Streams.PropertiesWindow
{
    public class Element
    {
        private string name;

        public string Name
        {
            get { return name; }
        }

        public Element(string name)
        {
            this.name = name;
        }
    }
}