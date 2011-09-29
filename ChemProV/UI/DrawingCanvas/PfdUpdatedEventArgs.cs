/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System;
using System.Collections.Generic;

using ChemProV.PFD;

namespace ChemProV.UI.DrawingCanvas
{
    public class PfdUpdatedEventArgs : EventArgs
    {
        public readonly IEnumerable<IPfdElement> pfdElements;

        public PfdUpdatedEventArgs(IEnumerable<IPfdElement> pfdElements)
        {
            this.pfdElements = pfdElements;
        }
    }
}