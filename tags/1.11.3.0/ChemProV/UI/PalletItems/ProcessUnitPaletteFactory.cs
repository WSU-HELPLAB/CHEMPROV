/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using ChemProV;

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
