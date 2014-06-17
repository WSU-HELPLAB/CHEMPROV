using System.Xml.Linq;

namespace ChemProV.Logic
{
    public class HeatExchangerWithUtility : AbstractProcessUnit
    {
        public HeatExchangerWithUtility()
            : this(AbstractProcessUnit.GetNextUID()) { }

        public HeatExchangerWithUtility(int id)
            : base(id, "Exc" + id.ToString()) { }

        public HeatExchangerWithUtility(XElement loadFromMe)
            : base(loadFromMe) { }

        public override string Description
        {
            get { return "Heat Exchanger With Utility"; }
        }

        public override bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty)
        {
            // These are only available on the highest difficulty setting
            return (OptionDifficultySetting.MaterialAndEnergyBalance == difficulty);
        }

        public override int MaxIncomingHeatStreams
        {
            get { return 1; }
        }

        public override int MaxIncomingStreams
        {
            get { return 1; }
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
                // The file format was setup this way a while back so we just have to stick with 
                // it to maintain compatibility.
                return "HeatExchanger";
            }
        }
    }
}
