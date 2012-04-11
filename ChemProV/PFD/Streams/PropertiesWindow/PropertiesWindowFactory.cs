/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using ChemProV.PFD.Streams.PropertiesWindow.Chemical;
using ChemProV.PFD.Streams.PropertiesWindow.Heat;

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
        public static IPropertiesWindow TableFromStreamObject(OptionDifficultySetting settings, bool isReadOnly, IStream stream)
        {
            if (stream is ChemicalStream)
            {
                return ChemicalStreamPropertiesTableFactory.GetChemicalStreamPropertiesTable(settings, isReadOnly, stream as ChemicalStream);
            }
            else if (stream is HeatStream)
            {
                return new HeatStreamPropertiesWindow(stream, isReadOnly);
            }
            //default case
            else
            {
                return new ChemicalStreamPropertiesWindow(stream, isReadOnly);
            }
        }

        /// <summary>
        /// Returns the appropriate properties table based on the supplied
        /// StreamType
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IPropertiesWindow TableFromStreamType(StreamType type, OptionDifficultySetting difficultySetting, bool isReadOnly)
        {
            switch (type)
            {
                case StreamType.Chemical:
                    if (difficultySetting == OptionDifficultySetting.MaterialAndEnergyBalance)
                    {
                        return new ChemicalStreamPropertiesWindowWithTemperature(isReadOnly);
                    }
                    else
                    {
                        return new ChemicalStreamPropertiesWindow(isReadOnly);
                    }

                case StreamType.Heat:
                    return new HeatStreamPropertiesWindow(isReadOnly);

                default:
                    return new ChemicalStreamPropertiesWindow(isReadOnly);
            }
        }

        public static IPropertiesWindow TableFromTable(IPropertiesWindow orginalTable, OptionDifficultySetting difficultySetting, bool isReadOnly)
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
                        IPropertiesWindow window = TableFromXml(xdoc.Element("ChemicalStreamPropertiesWindow"), difficultySetting, isReadOnly);

                        //this is questionable if this should be here, but since we copied the table we let the parent stream know that he has a new table to point at
                        orginalTable.ParentStream.Table = window;
                        window.ParentStream = orginalTable.ParentStream;
                        return window;
                    }
                }
            }
        }

        public static IPropertiesWindow TableFromXml(XElement tableXml, OptionDifficultySetting difficultySetting, bool isReadOnly)
        {
            //The root node name should be the name of the object to create
            string objectName = tableXml.Name.ToString();
            IPropertiesWindow table = null;
            if (objectName.CompareTo("ChemicalStreamPropertiesWindow") == 0)
            {
                table = TableFromStreamType(StreamType.Chemical, difficultySetting, isReadOnly);

                //find all data present in the xml
                (table as ChemicalStreamPropertiesWindow).ItemSource.Clear();

                var xmlData = from c in tableXml.Elements("DataRows").ElementAt(0).Elements("ChemicalStreamData")
                              select new
                              {
                                  label = (string)c.Element("Label"),
                                  quantity = (string)c.Element("Quantity"),
                                  unitId = (string)c.Element("UnitId"),
                                  compoundId = (string)c.Element("CompoundId"),
                                  enabled = (string)c.Element("Enabled")
                              };
                for (int i = 0; i < xmlData.Count(); i++)
                {
                    ChemicalStreamData d = new ChemicalStreamData();
                    d.Label = xmlData.ElementAt(i).label;
                    d.Quantity = xmlData.ElementAt(i).quantity;
                    d.UnitId = Convert.ToInt32(xmlData.ElementAt(i).unitId);
                    d.CompoundId = Convert.ToInt32(xmlData.ElementAt(i).compoundId);
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

                //check to see if temp has at least 1 element if it does not then temp/tempUnits was not saved and so they
                //will have default values.  If temp has 1(+) element(s) then we will load the first one
                if (temp.Count() != 0)
                {
                    (table as ChemicalStreamPropertiesWindow).ItemSource[0].Temperature = temp.ElementAt(0).quantity;
                    (table as ChemicalStreamPropertiesWindow).ItemSource[0].TempUnits = Convert.ToInt32(temp.ElementAt(0).units);
                }
                else
                {
                    (table as ChemicalStreamPropertiesWindow).ItemSource[0].Temperature = "T" + xmlData.ElementAt(0).label;
                }
            }
            else if (objectName.CompareTo("HeatStreamPropertiesWindow") == 0)
            {
                //AC: Introcuded as a "quck fix" for loading of heat streams.  However, it should be pretty
                //evident that there's a lot of overlap between the two options.  At a later time, it might
                //be fun to go back and make things cleaner.
                table = TableFromStreamType(StreamType.Heat, difficultySetting, isReadOnly);

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
                table = TableFromStreamType(StreamType.Generic, difficultySetting, isReadOnly);
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

            if (table is ChemicalStreamPropertiesWindow)
            {
                (table as ChemicalStreamPropertiesWindow).UpdateGrid();
            }

            return table;
        }
    }
}