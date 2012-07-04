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
using System.Xml.Linq;

namespace ChemProV.Core
{
    public class Mixer : AbstractProcessUnit
    {
        public Mixer()
            : this(AbstractProcessUnit.GetNextUID()) { }

        public Mixer(int id)
            : base(id, "Exc" + id.ToString()) { }
        
        public Mixer(XElement loadFromMe)
            : base(loadFromMe)
        {
            Label = "Mix" + Id.ToString();
        }

        public override string Description
        {
            get { return "Mixer"; }
        }

        public override bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty)
        {
            // Mixers are available at all difficulty settings
            return true;
        }

        public override int MaxIncomingHeatStreams
        {
            get { return 0; }
        }

        public override int MaxIncomingStreams
        {
            get
            {
                // -1 implies an infinite number of possible incoming streams
                return -1;
            }
        }

        public override int MaxOutgoingHeatStreams
        {
            get { return 0; }
        }

        public override int MaxOutgoingStreams
        {
            get { return 1; }
        }

        /// <summary>
        /// String for the identifier that gets written to the XML file. This string identifies 
        /// the type of process unit.
        /// </summary>
        public override string UnitTypeString
        {
            get
            {
                return "Mixer";
            }
        }
    }
}
