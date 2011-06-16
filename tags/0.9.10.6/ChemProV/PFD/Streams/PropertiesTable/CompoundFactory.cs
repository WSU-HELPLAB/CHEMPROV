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
using System.Collections.Generic;

namespace ChemProV.PFD.Streams.PropertiesTable
{
    public class CompoundFactory
    {
        private static Element hydrogren = new Element("H");
        private static Element oxygen = new Element("O");
        private static Element carbon = new Element("C");
        private static Element nitrogen = new Element("N");
        private static Element chlorine = new Element("Cl");
        private static Element phosphorus = new Element("P");
        private static Element sulfer = new Element("S");
        private static Element sodium = new Element("Na");

        public static List<KeyValuePair<Element, int>> getElementsOfCompound(string compoundName)
        {
            List<KeyValuePair<Element, int>> elements = new List<KeyValuePair<Element, int>>();
            switch (compoundName)
            {
                case "overall":
                        {
                            elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 0));
                            elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(oxygen, 0));
                            elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(carbon, 0));
                            elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(nitrogen, 0));
                            elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(chlorine, 0));
                            elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(phosphorus, 0));
                            elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(sulfer, 0));
                            elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(sodium, 0));
                        }
                        break;
                case "acetic acid":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(carbon, 2));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 4));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(oxygen, 2));
                    }
                    break;
                case "ammonia":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(nitrogen, 1));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 3));
                    } 
                    break;
                case "benzene":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(carbon, 6));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 6));
                    } 
                    break;
                case "carbon dioxide":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(carbon, 1));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(oxygen, 2));
                    } 
                    break;
                case "carbon monoxide":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(carbon, 1));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(oxygen, 1));
                    } 
                    break;
                case "cyclohexane":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(carbon, 6));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 12));
                    } 
                    break;
                case "ethane":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(carbon, 2));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 6));
                    } 
                    break;
                case "ethanol":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(carbon, 2));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 6));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(oxygen, 1));
                    } 
                    break;
                case "ethylene":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(carbon, 2));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 4));
                    } 
                    break;
                case "hydrochloric acid":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 1));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(chlorine, 1));
                    } 
                    break;
                case "hydrogen":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 2));
                    } 
                    break;
                case "methane":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(carbon, 1));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 4));
                    }
                    break;
                case "methanol":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(carbon, 1));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 4));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(oxygen, 1));
                    }
                    break;
                case "n-butane":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(carbon, 4));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 10));
                    }
                    break;
                case "n-hexane":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(carbon, 6));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 1));
                    } 
                    break;
                case "n-octane":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(carbon, 8));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 18));
                    }
                    break;
                case "nitrogen":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(nitrogen, 2));
                    }
                    break;
                case "oxygen":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(oxygen, 2));
                    }
                    break;
                case "phosphoric acid":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 3));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(phosphorus, 1));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(oxygen, 4));
                    }
                    break;
                case "propane":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(carbon, 3));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 8));
                    }
                    break;
                case "sodium hydroxide":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(sodium, 1));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(oxygen, 1));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 1));                     
                    }
                    break;
                case "sulfuric acid":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 2));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(oxygen, 4));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(sulfer, 1));
                    } 
                    break;
                case "toluene":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(carbon, 7));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 8));
                    }
                    break;
                case "water":
                    {
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(hydrogren, 2));
                        elements.Add(new System.Collections.Generic.KeyValuePair<Element, int>(oxygen, 1));
                    } 
                    break;
            }
            return elements;
        }
    }
}
