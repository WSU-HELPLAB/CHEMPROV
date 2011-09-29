/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

namespace ChemProV.PFD.Streams.PropertiesWindow.Chemical
{
    public static class ChemicalStreamPropertiesTableFactory
    {
        public static ChemicalStreamPropertiesWindow GetChemicalStreamPropertiesTable(OptionDifficultySetting currentDifficultySetting, bool isReadOnly, ChemicalStream stream)
        {
            if (currentDifficultySetting == OptionDifficultySetting.MaterialAndEnergyBalance)
            {
                return new ChemicalStreamPropertiesWindowWithTemperature(stream, isReadOnly);
            }
            else
            {
                return new ChemicalStreamPropertiesWindow(stream, isReadOnly);
            }
        }
    }
}