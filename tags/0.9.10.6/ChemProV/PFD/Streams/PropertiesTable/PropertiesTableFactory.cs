using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

using ChemProV.PFD.Streams.PropertiesTable.Chemical;
namespace ChemProV.PFD.Streams.PropertiesTable
{
    public class PropertiesTableFactory
    {
        /// <summary>
        /// Returns the appropriate properties table based on the supplied
        /// IStream object
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IPropertiesTable TableFromStreamObject(IStream stream)
        {
            if (stream is ChemicalStream)
            {
                return new ChemicalStreamPropertiesTable(stream);
            }
            //default case
            else
            {
                return new ChemicalStreamPropertiesTable(stream);
            }
        }

        /// <summary>
        /// Returns the appropriate properties table based on the supplied
        /// StreamType
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IPropertiesTable TableFromStreamType(StreamType type)
        {
            if (type == StreamType.Chemical)
            {
                return new ChemicalStreamPropertiesTable();
            }
            //default case
            else
            {
                return new ChemicalStreamPropertiesTable();
            }
        }

        public static IPropertiesTable TableFromXml(XElement tableXml)
        {
            //The root node name should be the name of the object to create
            string objectName = tableXml.Name.ToString();
            IPropertiesTable table = null;
            if (objectName.CompareTo("ChemicalStreamPropertiesTable") == 0)
            {
                table = TableFromStreamType(StreamType.Chemical);
                
                //find all data present in the xml
                (table as ChemicalStreamPropertiesTable).ItemSource.Clear();

                var xmlData = from c in tableXml.Elements("DataRows").ElementAt(0).Elements("ChemicalStreamData")
                              select new
                              {
                                  label = (string)c.Element("Label"),
                                  quantity = (string)c.Element("Quantity"),
                                  units = (string)c.Element("Units"),
                                  compound = (string)c.Element("Compound"),
                                  enabled = (string)c.Element("Enabled")
                              };
                for (int i = 0; i < xmlData.Count(); i++ )
                {
                    ChemicalStreamData d = new ChemicalStreamData();
                    d.Label = xmlData.ElementAt(i).label;
                    d.Quantity = xmlData.ElementAt(i).quantity;
                    d.Units = Convert.ToInt32(xmlData.ElementAt(i).units);
                    d.Compound = Convert.ToInt32(xmlData.ElementAt(i).compound);
                    d.Enabled = Convert.ToBoolean(xmlData.ElementAt(i).enabled);
                    d.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler((table as ChemicalStreamPropertiesTable).DataUpdated);
                    (table as ChemicalStreamPropertiesTable).ItemSource.Add(d);
                }
            }
            else
            {
                table = TableFromStreamType(StreamType.Generic);
            }

            //set the table's location
            UIElement tableUi = table as UIElement;
            var location = from c in tableXml.Elements("Location")
                           select new
                           {
                               x = (string)c.Element("X"),
                               y = (string)c.Element("Y")
                           };
            tableUi.SetValue(Canvas.LeftProperty, Convert.ToDouble(location.ElementAt(0).x));
            tableUi.SetValue(Canvas.TopProperty, Convert.ToDouble(location.ElementAt(0).y));

            return table;
        }
    }
}
