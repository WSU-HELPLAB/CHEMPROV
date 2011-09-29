/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;

namespace ChemProV.UI.PalletItems
{
    public class ProcessUnitPaletteMaterialAndEnergyBalance : ProcessUnitPaletteMaterialBalancesWithReactors
    {
        public ProcessUnitPaletteMaterialAndEnergyBalance()
        {
            this.ProcessUnit_StackPanel.Children.Add(new ProcessUnitPaletteItem() { ProcessUnit = ProcessUnitType.HeatExchanger });
            this.ProcessUnit_StackPanel.Children.Add(new ProcessUnitPaletteItem() { ProcessUnit = ProcessUnitType.HeatExchangerNoUtility });
            this.Streams_StackPanel.Children.Add(new StreamPaletteItem() { Stream = StreamType.Heat });
            base.AttachMouseListenersToAllPaletteItems();
        }
    }
}