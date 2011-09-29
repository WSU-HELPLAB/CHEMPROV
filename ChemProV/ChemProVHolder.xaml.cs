/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/
﻿using System;
using System.IO;
using System.Windows.Controls;
using ChemProV.PFD.Streams.PropertiesWindow.Chemical;
using ChemProV.PFD.Streams.PropertiesWindow.Heat;
using ChemProV.Validation.Rules;

namespace ChemProV
{
    public partial class ChemProVHolder : UserControl
    {
        public ChemProVHolder()
        {
            InitializeComponent();

            AddNewMainPage();
        }

        private void RequestOpenFile(object sender, RequestOpenFileArgs e)
        {
            RemoveOldMainPage(sender as MainPage);
            AddNewMainPage(e.fileInfo);
        }

        private void newBlankChemProV(object sender, EventArgs e)
        {
            RemoveOldMainPage(sender as MainPage);
            AddNewMainPage();
        }

        private void RemoveOldMainPage(MainPage mp)
        {
            mp.Dispose();
            mp.RequestNewBlankMainPage -= new EventHandler(newBlankChemProV);
            mp.RequestOpenFile -= new RequestOpenFileEventHandler(RequestOpenFile);

            //since we just got rid of an old page we also need to reset the table counters
            ChemicalStreamPropertiesWindow.ResetTableCounter();
            HeatStreamPropertiesWindow.ResetTableCounter();

            this.LayoutRoot.Children.Remove(mp);
        }

        private void AddNewMainPage(FileInfo fileInfo = null)
        {
            MainPage mp = new MainPage(fileInfo);

            mp.RequestNewBlankMainPage += new EventHandler(newBlankChemProV);
            mp.RequestOpenFile += new RequestOpenFileEventHandler(RequestOpenFile);
            this.LayoutRoot.Children.Add(mp);
        }
    }
}