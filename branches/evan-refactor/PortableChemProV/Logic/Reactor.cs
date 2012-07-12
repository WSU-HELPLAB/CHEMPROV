using System;
using System.Net;
using System.Xml.Linq;

namespace ChemProV.Logic
{
    public class Reactor : AbstractProcessUnit
    {
        public Reactor()
            : this(AbstractProcessUnit.GetNextUID()) { }

        public Reactor(int id)
            : base(id, "Rct" + id.ToString()) { }

        public Reactor(XElement loadFromMe)
            : base(loadFromMe) { }

        public override string Description
        {
            get { return "Reactor"; }
        }

        public override bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty)
        {
            // Reactors are available on everything but the easiest difficulty setting
            return (OptionDifficultySetting.MaterialBalance != difficulty);
        }

        public override int MaxIncomingHeatStreams
        {
            get { return 1; }
        }

        public override int MaxIncomingStreams
        {
            get { return -1; }
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
                return "Reactor";
            }
        }
    }
}
