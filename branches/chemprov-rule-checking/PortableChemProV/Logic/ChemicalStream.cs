using System.Collections.Generic;
using System.Xml.Linq;

namespace ChemProV.Logic
{
    public class ChemicalStream : AbstractStream
    {
        public ChemicalStream(int uniqueId) : base(uniqueId) { }

        public ChemicalStream(XElement loadFromMe, IList<AbstractProcessUnit> processUnits)
            : base(loadFromMe, processUnits) { }

        public override bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty)
        {
            // Chemical streams are available with all difficulties
            return true;
        }

        public override bool IsValidSource(AbstractProcessUnit unit)
        {
            // Chemical streams can accept any type of process unit as a source, provided 
            // that the unit is accepting outgoing streams.
            // TODO: Double check this with the chemistry guys
            return unit.CanAcceptOutgoingStream(this);
        }

        public override bool IsValidDestination(AbstractProcessUnit unit)
        {
            // As far as I can tell, the chemical streams can accept any type of process 
            // unit as a destination. So the only required check is that the unit is 
            // accepting incoming streams.
            // TODO: Double check this with the chemistry guys
            return unit.CanAcceptIncomingStream(this);
        }

        /// <summary>
        /// The string type that gets written to files and identifies the type of 
        /// of inheriting class.
        /// </summary>
        public override string StreamType
        {
            get
            {
                return "Chemical";
            }
        }

        public override string Title
        {
            get
            {
                return "Chemical Stream";
            }
        }
    }
}
