using System;
using System.Xml.Linq;

namespace ChemProV.Logic
{
    public static class ProcessUnitFactory
    {
        public static AbstractProcessUnit Create(XElement element)
        {
            switch (element.Attribute("ProcessUnitType").Value)
            {
                case "HeatExchanger":
                    return new HeatExchangerWithUtility(element);

                case "HeatExchangerNoUtility":
                    return new HeatExchangerNoUtility(element);

                case "Mixer":
                    return new Mixer(element);

                case "Reactor":
                    return new Reactor(element);

                case "Separator":
                    return new Separator(element);
                    
                default:
                    throw new Exception(
                        "Unknown process unit type: " + element.Attribute("ProcessUnitType").Value);
            }
        }
    }
}
