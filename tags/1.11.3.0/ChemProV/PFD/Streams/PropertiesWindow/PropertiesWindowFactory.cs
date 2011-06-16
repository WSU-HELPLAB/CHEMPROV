/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/
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
using System.Xml.Serialization;
using System.Xml.Linq;
using System.IO.IsolatedStorage;
using System.IO;

using ChemProV;
using ChemProV.PFD.Streams.PropertiesWindow.Heat;
using ChemProV.PFD.Streams.PropertiesWindow.Chemical;

namespace ChemProV.PFD.Streams.PropertiesWindow
{
    public class PropertiesWindowFactory
    {
        /// <summary>
        /// Returns the appropriate properties table based on the supplied
        /// IStream object
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IPropertiesWindow TableFromStreamObject(OptionDifficultySetting settings, IStream stream)
        {
            if (stream is ChemicalStream)
            {
                return ChemicalStreamPropertiesTableFactory.GetChemicalStreamPropertiesTable(settings, stream as ChemicalStream);
            }
            else if (stream is HeatStream)
            {
                return new HeatStreamPropertiesWindow(stream);
            }
            //default case
            else
            {
                return new ChemicalStreamPropertiesWindow(stream);
            }
        }

        /// <summary>
        /// Returns the appropriate properties table based on the supplied
        /// StreamType
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IPropertiesWindow TableFromStreamType(StreamType type, OptionDifficultySetting difficultySetting)
        {
            switch (type)
            {
                case StreamType.Chemical:
                    if(difficultySetting == OptionDifficultySetting.MaterialAndEnergyBalance)
                    {
                        return new ChemicalStreamPropertiesWindowWithTemperature();
                    }
                    else
                    {
                        return new ChemicalStreamPropertiesWindow();
                    }

                case StreamType.Heat:
                    return new HeatStreamPropertiesWindow();

                default:
                    return new ChemicalStreamPropertiesWindow();
            }
        }

        public static IPropertiesWindow TableFromTable(IPropertiesWindow orginalTable, OptionDifficultySetting difficultySetting)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream("Temp.xml", FileMode.Create, isf))
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = "   ";
                    XmlWriter writer = XmlWriter.Create(isfs, settings);
                    //writer.WriteStartElement("ChemicalStreamPropertiesWindow");
                    (new XmlSerializer(typeof(ChemicalStreamPropertiesWindow))).Serialize(writer, orginalTable);
                    //writer.WriteEndElement();
                    writer.Flush();
                }
                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream("Temp.xml", FileMode.Open, isf))
                {
                    using (StreamReader sr = new StreamReader(isfs))
                    {
                        XDocument xdoc = XDocument.Load(sr);
                        IPropertiesWindow window = TableFromXml(xdoc.Element("ChemicalStreamPropertiesWindow"), difficultySetting);
                        
                        //this is questionable if this should be here, but since we copied the table we let the parent stream know that he has a new table to point at
                        orginalTable.ParentStream.Table = window;
                        window.ParentStream = orginalTable.ParentStream;

                        return window;
                    }
                }
            }
        }

        public static IPropertiesWindow TableFromXml(XElement tableXml, OptionDifficultySetting difficultySetting)
        {
            //The root node name should be the name of the object to create
            string objectName = tableXml.Name.ToString();
            IPropertiesWindow table = null;
            if (objectName.CompareTo("ChemicalStreamPropertiesWindow") == 0)
            {
                 table = TableFromStreamType(StreamType.Chemical, difficultySetting);
                
                //find all data present in the xml
                 if (table is ChemicalStreamPropertiesWindow)
                 {
                     (table as ChemicalStreamPropertiesWindow).ItemSource.Clear();
                 }
                 else
                 {
                     (table as ChemicalStreamPropertiesWindowWithTemperature).ItemSource.Clear();
                 }
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
                    d.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler((table as ChemicalStreamPropertiesWindow).DataUpdated);
                    (table as ChemicalStreamPropertiesWindow).ItemSource.Add(d);
                }
                var temp = from c in tableXml.Elements("Temperature")
                               select new
                               {
                                   quantity = (string)c.Element("Quantity"),
                                   units = (string)c.Element("Units"),
                               };
                (table as ChemicalStreamPropertiesWindow).ItemSource[0].Temperature = temp.ElementAt(0).quantity;
                (table as ChemicalStreamPropertiesWindow).ItemSource[0].TempUnits = Convert.ToInt32(temp.ElementAt(0).units);
            }
            else if (objectName.CompareTo("HeatStreamPropertiesWindow") == 0)
            {
                //AC: Introcuded as a "quck fix" for loading of heat streams.  However, it should be pretty
                //evident that there's a lot of overlap between the two options.  At a later time, it might
                //be fun to go back and make things cleaner.
                table = TableFromStreamType(StreamType.Heat, difficultySetting);
                
                //find all data present in the xml
                (table as HeatStreamPropertiesWindow).ItemSource.Clear();

                var xmlData = from c in tableXml.Elements("DataRows").ElementAt(0).Elements("HeatStreamData")
                              select new
                              {
                                  label = (string)c.Element("Label"),
                                  quantity = (string)c.Element("Quantity"),
                                  units = (string)c.Element("Units"),
                                  enabled = (string)c.Element("Enabled")
                              };
                for (int i = 0; i < xmlData.Count(); i++)
                {
                    HeatStreamData d = new HeatStreamData();
                    d.Label = xmlData.ElementAt(i).label;
                    d.Quantity = xmlData.ElementAt(i).quantity;
                    d.Units = Convert.ToInt32(xmlData.ElementAt(i).units);
                    d.Enabled = Convert.ToBoolean(xmlData.ElementAt(i).enabled);
                    d.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler((table as HeatStreamPropertiesWindow).DataUpdated);
                    (table as HeatStreamPropertiesWindow).ItemSource.Add(d);
                }
            }
            else
            {
                table = TableFromStreamType(StreamType.Generic, difficultySetting);
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
