/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
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