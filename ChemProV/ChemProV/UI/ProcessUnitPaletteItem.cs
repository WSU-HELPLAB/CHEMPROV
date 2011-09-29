/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ChemProV.PFD.ProcessUnits;

namespace ChemProV.UI
{
    /// <summary>
    /// This is used in the Palette for all ProcessUnits
    /// </summary>
    public class ProcessUnitPaletteItem : GenericPaletteItem
    {

        //Dependency region declarations for setting palette item options.
        //Coolness.
        #region dependency properties

        /// <summary>
        /// used to set the process unit attached to the palette item
        /// </summary>
        public static readonly DependencyProperty ProcessUnitProperty;

        #endregion

        //where traditional C# properties (GET/SET) go
        #region properties

        /// <summary>
        /// This gets or sets the ProcessUnitType
        /// </summary>
        public ProcessUnitType ProcessUnit
        {
            get
            {
                return (ProcessUnitType)GetValue(ProcessUnitProperty);
            }
            set
            {
                SetValue(ProcessUnitProperty, value);
            }
        }

        /// <summary>
        /// This is a reference to a ProcessUnit
        /// </summary>
        public IProcessUnit LocalProcessUnit
        {
            get
            {
                return (IProcessUnit)data;
            }
            set
            {
                data = value;
            }
        }

        #endregion

        //where we put event listeners
        #region event listeneres

        /// <summary>
        /// Will be called whenever someone makes a change to the "ProcessUnit" property
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnProcessUnitPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //cast the new value as a ProcessUnitType enum
            ProcessUnitType type = (ProcessUnitType)e.NewValue;

            //turn the DependencyObject into a ProcessUnitPaletteItem
            ProcessUnitPaletteItem item = (ProcessUnitPaletteItem)d;

            //get the associated process unit type
            IProcessUnit pu = ProcessUnitFactory.ProcessUnitFromUnitType(type);
            item.LocalProcessUnit = pu;

            //should use data binding, but meh
            item.IconSource = ((BitmapImage)pu.Icon.Source).UriSource.OriginalString;
            item.Description = pu.Description;
        }

        #endregion

        /// <summary>
        /// Static constructor that sets up dependency properties and other goodies.
        /// </summary>
        static ProcessUnitPaletteItem()
        {
            //initialize the class' dependency properties
            ProcessUnitProperty = DependencyProperty.Register(
                                                     "ProcessUnit",
                                                     typeof(ProcessUnitType),
                                                     typeof(ProcessUnitPaletteItem),
                                                     new PropertyMetadata(
                                                         ProcessUnitType.Generic,
                                                         new PropertyChangedCallback(OnProcessUnitPropertyChange)
                                                         )
                                                     );


        } 
    }
}
