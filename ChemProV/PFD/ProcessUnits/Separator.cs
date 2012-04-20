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

namespace ChemProV.PFD.ProcessUnits
{
    public class Separator : LabeledProcessUnit
    {
        public Separator()
            : base("/UI/Icons/pu_separator.png")
        {

        }

        /// <summary>
        /// Gets the title that will appear in the control palette
        /// </summary>
        public override string Description
        {
            get
            {
                return "Separator";
            }
        }

        public override bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty)
        {
            // Separators are available at all difficulty settings
            return true;
        }

        public override int MaxIncomingStreams
        {
            get
            {
                return 1;
            }
        }

        public override int MaxOutgoingStreams
        {
            get
            {
                return -1;
            }
        }

        public override int MaxIncomingHeatStreams
        {
            get
            {
                return 0;
            }
        }

        public override int MaxOutgoingHeatStreams
        {
            get
            {
                return 0;
            }
        }
    }
}
