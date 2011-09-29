/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChemProV.UI.PalletItems
{
    /// <summary>
    /// This class defines base members for all PaletteItems
    /// </summary>
    public partial class GenericPaletteItem : UserControl, IPaletteItem
    {
        //instance variables

        #region instance variables

        private static Color SelectedColor = Colors.Yellow;
        private static Color UnSelectedColor = Colors.White;

        /// <summary>
        /// reference to the object that the paletteItem represents
        /// </summary>
        protected object data = null;

        #endregion instance variables

        //Dependency region declarations for setting palette options.
        //Coolness.

        #region dependency properties

        /// <summary>
        /// to change the palette icon
        /// </summary>
        public static readonly DependencyProperty IconSourceProperty;

        /// <summary>
        /// to change the palette description
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty;

        /// <summary>
        /// determines highlighting for the current object
        /// </summary>
        public static readonly DependencyProperty SelectedProperty;

        #endregion dependency properties

        //where traditional C# properties (GET/SET) go

        #region properties

        /// <summary>
        /// Sets the locaiton of the palette object's icon.
        /// </summary>
        public String IconSource
        {
            get
            {
                return (String)GetValue(IconSourceProperty);
            }
            set
            {
                SetValue(IconSourceProperty, value);
            }
        }

        /// <summary>
        /// Sets the palette object's descriptor text.  Will appear to the right of
        /// the palette's icon.
        /// </summary>
        public String Description
        {
            get
            {
                return (String)GetValue(DescriptionProperty);
            }
            set
            {
                SetValue(DescriptionProperty, value);
            }
        }

        /// <summary>
        /// Sets whether or not the current palette item is selected or now
        /// </summary>
        public bool Selected
        {
            get
            {
                return (bool)GetValue(SelectedProperty);
            }
            set
            {
                SetValue(SelectedProperty, value);
            }
        }

        /// <summary>
        /// Used as a hack to store further information related to a palette item.
        /// Implemented specifically to store the ProcessUnit type in the
        /// ProcessUnitPaletteItem class.
        /// </summary>
        public object Data
        {
            get
            {
                return data;
            }
        }

        #endregion properties

        //where we put event listeners

        #region event listeneres

        /// <summary>
        /// Will be called whenever someone makes a change to the "IconSource" property
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnIconSourcePropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GenericPaletteItem p = (GenericPaletteItem)d;

            //create a new bitmap image, assign it to the GenericPaletteItem's image source
            //property
            BitmapImage bi = new BitmapImage();
            bi.UriSource = new Uri(e.NewValue.ToString(), UriKind.Relative);
            p.PaletteIcon.Source = bi;
        }

        /// <summary>
        /// Will be called whenenver something makes a change to the "Description" property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnDescriptionPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GenericPaletteItem p = (GenericPaletteItem)d;

            //set the palette object's description to the updated text
            p.PaletteDescription.Text = p.Description;
        }

        /// <summary>
        /// Will be called whenenver something makes a change to the "Selected" property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnSelectedPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GenericPaletteItem item = (GenericPaletteItem)d;

            bool value = (bool)e.NewValue;

            //if value is true, then the current item has been selected
            if (value)
            {
                item.LayoutRoot.Background = new SolidColorBrush(SelectedColor);
            }
            else
            {
                item.LayoutRoot.Background = new SolidColorBrush(UnSelectedColor);
            }
        }

        #endregion event listeneres

        /// <summary>
        /// Static constructor that sets up dependency properties and other goodies.
        /// </summary>
        static GenericPaletteItem()
        {
            //initialize the class' dependency properties
            IconSourceProperty = DependencyProperty.Register(
                                                     "IconSource",
                                                     typeof(String),
                                                     typeof(GenericPaletteItem),
                                                     new PropertyMetadata(
                                                         "",
                                                         new PropertyChangedCallback(OnIconSourcePropertyChange)
                                                         )
                                                     );
            DescriptionProperty = DependencyProperty.Register(
                                                     "Description",
                                                     typeof(String),
                                                     typeof(GenericPaletteItem),
                                                     new PropertyMetadata(
                                                         "",
                                                         new PropertyChangedCallback(OnDescriptionPropertyChange)
                                                         )
                                                     );
            SelectedProperty = DependencyProperty.Register(
                                                     "Selected",
                                                     typeof(bool),
                                                     typeof(GenericPaletteItem),
                                                     new PropertyMetadata(
                                                         false,
                                                         new PropertyChangedCallback(OnSelectedPropertyChange)
                                                         )
                                                     );
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public GenericPaletteItem()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Implementation of IComparable.CompareTo method.  For the sake of a palette item,
        /// two elements are equal when they have the same icon source and description.
        /// Note that this method isn't set up to do value comparisons (greater / less than)
        /// as that doesn't make sense in this context.
        /// </summary>
        /// <param name="obj">The other palette item used in the comparison</param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            //if we're dealing with something that isn't the same class, then they're
            //definately not the same
            if (!(obj is IPaletteItem))
            {
                return -1;
            }

            //as noted in the method header, the two objects are equal when their icon
            //sorce and descriptions match
            IPaletteItem otherPaletteItem = (IPaletteItem)obj;
            if (this.IconSource == otherPaletteItem.IconSource
                &&
               this.Description.CompareTo(otherPaletteItem.Description) == 0
                )
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
    }
}