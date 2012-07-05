using System;
using System.Xml.Linq;

namespace ChemProV.Core
{
    public class Separator : AbstractProcessUnit
    {
        public Separator()
            : this(AbstractProcessUnit.GetNextUID()) { }

        public Separator(int id)
            : base(id, "Sep" + id.ToString()) { }
        
        public Separator(XElement loadFromMe)
            : base(loadFromMe)
        {
            Label = "Sep" + Id.ToString();
        }

        public override string Description
        {
            get { return "Separator"; }
        }

        public override bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty)
        {
            // Separators are available at all difficulty settings
            return true;
        }

        public override int MaxIncomingHeatStreams
        {
            get { return 0; }
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
            get { return -1; }
        }

        /// <summary>
        /// String for the identifier that gets written to the XML file. This string identifies 
        /// the type of process unit.
        /// </summary>
        public override string UnitTypeString
        {
            get
            {
                return "Separator";
            }
        }
    }
}
