using System.Xml.Linq;

namespace ChemProV.Core
{
    public class HeatExchangerNoUtility : AbstractProcessUnit
    {
        public HeatExchangerNoUtility()
            : this(AbstractProcessUnit.GetNextUID()) { }
        
        public HeatExchangerNoUtility(int id)
            : base(id, "Exc" + id.ToString()) { }
        
        public HeatExchangerNoUtility(XElement loadFromMe)
            : base(loadFromMe)
        {
            Label = "Exc" + Id.ToString();
        }

        public override string Description
        {
            get { return "Heat Exchanger Without Utility"; }
        }

        public override bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty)
        {
            // These are only available on the highest difficulty setting
            return (OptionDifficultySetting.MaterialAndEnergyBalance == difficulty);
        }

        public override int MaxIncomingHeatStreams
        {
            get { return 0; }
        }

        public override int MaxIncomingStreams
        {
            get { return 2; }
        }

        public override int MaxOutgoingHeatStreams
        {
            get { return 0; }
        }

        public override int MaxOutgoingStreams
        {
            get { return 2; }
        }

        /// <summary>
        /// String for the identifier that gets written to the XML file. This string identifies 
        /// the type of process unit.
        /// </summary>
        public override string UnitTypeString
        {
            get
            {
                return "HeatExchangerNoUtility";
            }
        }
    }
}
