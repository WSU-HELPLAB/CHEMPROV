using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ChemProV.PFD.ProcessUnits
{
    public partial class LabeledProcessUnit : GenericProcessUnit
    {
        public LabeledProcessUnit() : base()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        #region GenericProcessUnitOverrides

        /// <summary>
        /// Gets/Sets the icon dependency property
        /// </summary>
        public override Image Icon
        {
            get
            {
                return ProcessUnitImage;
            }
            set
            {
                ProcessUnitImage.Source = value.Source;
            }
        }

        /// <summary>
        /// Use to reference the border around the process unit's icon.  
        /// </summary>
        public override Border IconBorder
        {
            get
            {
                return ProcessUnitBorder;
            }
            set
            {
                ProcessUnitBorder = value;
            }
        }

        #endregion

    }
}
