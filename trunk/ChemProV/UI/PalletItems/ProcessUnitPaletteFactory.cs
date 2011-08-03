/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

namespace ChemProV.UI.PalletItems
{
    public static class ProcessUnitPaletteFactory
    {
        public static ProcessUnitPalette GetProcessUnitPalette(OptionDifficultySetting currentDifficultySetting)
        {
            ProcessUnitPalette processUnitPalette = null;
            switch (currentDifficultySetting)
            {
                case OptionDifficultySetting.MaterialBalance:
                    processUnitPalette = new ProcessUnitPaletteMaterialBalances();
                    break;
                case OptionDifficultySetting.MaterialBalanceWithReactors:
                    processUnitPalette = new ProcessUnitPaletteMaterialBalancesWithReactors();
                    break;
                case OptionDifficultySetting.MaterialAndEnergyBalance:
                    processUnitPalette = new ProcessUnitPaletteMaterialAndEnergyBalance();
                    break;
            }
            return processUnitPalette;
        }
    }
}