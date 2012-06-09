/*
Copyright 2010 - 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

// If I had it my way, I'd rename the sticky note Silverlight user control to StickyNoteControl and 
// have this class just be named StickyNote, but such major changes are trying to be avoided at this 
// point in time.

namespace ChemProV.PFD.StickyNote
{
    /// <summary>
    /// This class exists to provide a UI-independent sticky note entity. This is important because 
    /// things like ASP.NET pages, WinForms apps, etc. cannot load Silverlight controls or reference 
    /// Silverlight class libraries. So if we have all the logic for PFD elements within Silverlight 
    /// controls then we can't use it in non-Silverlight apps.
    /// This class can load/save/store all relevant data for a sticky note without any Silverlight-
    /// specific dependencies.
    /// </summary>
    public class StickyNote_UIIndependent
    {
        private double m_locX = 0.0;

        private double m_locY = 0.0;

        private string m_parentId = null;

        private double m_sizeH = 100.0;

        private double m_sizeW = 100.0;

        private string m_text = null;

        private string m_userName = null;

        public StickyNote_UIIndependent() { }

        /// <summary>
        /// Initializes from a &lt;StickyNote&gt; or &lt;Comment&gt; XElement with an optional ideintifier 
        /// for a parent object.
        /// </summary>
        public StickyNote_UIIndependent(XElement stickyNoteElement, string optionalParentId)
        {
            // Make sure we have the correct type of XElement
            string elNameLwr = stickyNoteElement.Name.LocalName.ToLower();
            if (!elNameLwr.Equals("stickynote") && !elNameLwr.Equals("comment"))
            {
                throw new ArgumentException("XElement for a sticky note must be either a " +
                    "<Comment> or <StickyNote> element. Element was named: " + elNameLwr);
            }
            
            m_parentId = optionalParentId;
            m_text = stickyNoteElement.Element("Content").Value;

            //use LINQ to find us the X,Y coords
            var location = from c in stickyNoteElement.Elements("Location")
                           select new
                           {
                               x = (string)c.Element("X"),
                               y = (string)c.Element("Y")
                           };
            m_locX = Convert.ToDouble(location.ElementAt(0).x);
            m_locY = Convert.ToDouble(location.ElementAt(0).y);

            // E.O.
            // Load the size information. If it is not present, default to 100x100
            XElement sizeEl = stickyNoteElement.Element("Size");
            if (null == sizeEl)
            {
                m_sizeH = m_sizeW = 100.0;
            }
            else
            {
                if (!TryParseXY(sizeEl.Value, out m_sizeW, out m_sizeH))
                {
                    m_sizeH = m_sizeW = 100.0;
                }
            }

            // Look for a user name
            XElement userEl = stickyNoteElement.Element("UserName");
            if (null != userEl)
            {
                m_userName = userEl.Value;
            }
        }

        public double Height
        {
            get
            {
                return m_sizeH;
            }
        }

        public double LocationX
        {
            get
            {
                return m_locX;
            }
        }

        public double LocationY
        {
            get
            {
                return m_locY;
            }
        }

        public string ParentId
        {
            get
            {
                return m_parentId;
            }
        }

        public string Text
        {
            get
            {
                return m_text;
            }
        }

        /// <summary>
        /// Attempts to parse a string of the form "X,Y". On failure, the values are set 
        /// to 0.0 and false is returned.
        /// </summary>
        private static bool TryParseXY(string pointString, out double x, out double y)
        {
            // The expected format of the string is "X,Y"
            if (null == pointString || !pointString.Contains(","))
            {
                x = y = 0.0;
                return false;
            }

            string[] components = pointString.Split(',');
            if (null == components || components.Length < 2)
            {
                x = y = 0.0;
                return false;
            }

            if (double.TryParse(components[0], out x) && double.TryParse(components[1], out y))
            {
                return true;
            }

            x = y = 0.0;
            return false;
        }

        public string UserName
        {
            get
            {
                return m_userName;
            }
            set
            {
                m_userName = value;
            }
        }

        public double Width
        {
            get
            {
                return m_sizeW;
            }
        }

        /// <summary>
        /// Writes this sticky note's XML as a child under the specified parent
        /// </summary>
        public void WriteElement(XElement parent)
        {
            XElement tree = new XElement("Comment",
                new XElement("Location", 
                    new XElement("X", m_locX),
                    new XElement("Y", m_locY)),
                new XElement("Content", m_text),
                new XElement("Size", string.Format("{0},{1}", Width, Height)));

            // Write the user name if we have one
            if (!string.IsNullOrEmpty(m_userName))
            {
                tree.Add(new XElement("UserName", m_userName));
            }

            parent.Add(tree);
        }
    }
}
