using System.Collections.Generic;
using System.Xml.Linq;

namespace ChemProV.Core
{
    public class HeatStream : AbstractStream
    {
        public HeatStream() : base() { }
        
        public HeatStream(int uniqueId) : base(uniqueId) { }

        public HeatStream(XElement loadFromMe, IList<AbstractProcessUnit> processUnits)
            : base(loadFromMe, processUnits) { }

        public override bool IsAvailableWithDifficulty(OptionDifficultySetting difficulty)
        {
            // Heat streams are only available with MaterialAndEnergyBalance
            return (OptionDifficultySetting.MaterialAndEnergyBalance == difficulty);
        }

        public override bool IsValidSource(AbstractProcessUnit unit)
        {
            return false;
        }

        public override bool IsValidDestination(AbstractProcessUnit unit)
        {
            // Heat streams can only have reactors as destinations (and of course the 
            // reactor unit has to be accepting incoming streams).
            // TODO: Check with the chemistry guys to verify this
            return ((unit is Reactor) && unit.CanAcceptIncomingStream(this));
        }

        /// <summary>
        /// The string type that gets written to files and identifies the type of 
        /// of inheriting class.
        /// </summary>
        public override string StreamType
        {
            get
            {
                return "Heat";
            }
        }

        public override string Title
        {
            get
            {
                return "Heat Stream";
            }
        }
    }
}
