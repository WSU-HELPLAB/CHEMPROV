/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

namespace ChemProV.Logic.Equations
{
    public interface IEquationModifier
    {
        int ClassificationId { get; }
        string Name { get; set; }
    }
}
