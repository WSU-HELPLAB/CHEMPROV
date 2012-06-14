/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

namespace ChemProV.PFD.Streams.PropertiesWindow.Chemical
{
    public static class ChemicalStreamPropertiesTableFactory
    {
        public static ChemicalStreamPropertiesWindow GetChemicalStreamPropertiesTable(OptionDifficultySetting currentDifficultySetting, bool isReadOnly, ChemicalStream stream)
        {
            if (currentDifficultySetting == OptionDifficultySetting.MaterialAndEnergyBalance)
            {
                return new ChemicalStreamPropertiesWindow(stream, isReadOnly, true);
            }
            else
            {
                return new ChemicalStreamPropertiesWindow(stream, isReadOnly);
            }
        }
    }
}