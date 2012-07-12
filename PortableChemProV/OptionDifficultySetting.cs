/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

namespace ChemProV
{
    /// <summary>
    /// Each of these builds of the previous therefore order matters.
    /// The first (0) is the simplest the last is the most complex
    /// </summary>
    public enum OptionDifficultySetting
    {
        MaterialBalance = 0,
        MaterialBalanceWithReactors = 1,
        MaterialAndEnergyBalance = 2
    }
}
