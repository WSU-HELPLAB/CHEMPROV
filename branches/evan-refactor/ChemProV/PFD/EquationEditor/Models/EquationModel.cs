using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using System.Xml.Serialization;
using ChemProV.Core;

namespace ChemProV.PFD.EquationEditor.Models
{
    public class EquationModel : INotifyPropertyChanged, IXmlSerializable
    {
        #region public members
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public IList<IPfdElement> RelatedElements { get; set; }
        #endregion

        #region private members
        private static int _staticId = 0;

        private int _id;
        private EquationScope _scope = new EquationScope();
        private EquationType _type = new EquationType();
        private string _equation = "";

        /// <summary>
        /// List of comments for this equation
        /// </summary>
        private List<Core.BasicComment> m_comments = new List<Core.BasicComment>();

        #endregion

        #region properties
        public int Id 
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                if (_id > _staticId)
                {
                    _staticId = _id + 1;
                }
                OnPropertyChanged("Id");
            }
        }

        public IList<Core.BasicComment> Comments
        {
            get
            {
                return m_comments;
            }
        }

        public EquationScope Scope
        {
            get
            {
                return _scope;
            }
            set
            {
                _scope = value;
                OnPropertyChanged("Scope");
            }
        }
        public EquationType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                OnPropertyChanged("Type");
            }
        }
        public string Equation
        {
            get
            {
                return _equation;
            }
            set
            {
                _equation = value;
                OnPropertyChanged("Equation");
            }
        }
        #endregion

        #region public methods
        public EquationModel()
        {
            _staticId++;
            Id = _staticId;
            RelatedElements = new List<IPfdElement>();
        }

        #region serialization methods
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            //not needed
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("Id", Id.ToString());

            writer.WriteStartElement("Scope");
            writer.WriteAttributeString("Name", Scope.Name);
            writer.WriteAttributeString("ClassificationId", Scope.ClassificationId.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("Type");
            writer.WriteAttributeString("Name", Type.Name);
            writer.WriteAttributeString("ClassificationId", Type.ClassificationId.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("Equation");
            writer.WriteString(Equation);
            writer.WriteEndElement();

            // Write all comments. Older versions used an "Annotation" tag for a single comment. A redesign 
            // gave support for multiple comments but we still use the "Annotation" tag for compatibility.
            foreach (Core.BasicComment bc in m_comments)
            {
                writer.WriteStartElement("Annotation");
                if (!string.IsNullOrEmpty(bc.CommentUserName))
                {
                    writer.WriteAttributeString("UserName", bc.CommentUserName);
                }
                writer.WriteString(bc.CommentText);
                writer.WriteEndElement();
            }
        }

        public static EquationModel FromXml(XElement xmlModel)
        {
            EquationModel model = new EquationModel();

            //easy stuff first
            int id = 0;
            Int32.TryParse(xmlModel.Attribute("Id").Value, out id);
            if (id != 0)
            {
                model.Id = id;
            }
            model.Equation = xmlModel.Element("Equation").Value;
            //model.Annotation = xmlModel.Element("Annotation").Value;

            // Read in the comments
            foreach (XElement cmtsEl in xmlModel.Elements("Annotation"))
            {
                XAttribute userAttr = cmtsEl.Attribute("UserName");
                string userName = (null == userAttr) ? null : userAttr.Value;
                model.m_comments.Add(new Core.BasicComment(cmtsEl.Value, userName));
            }

            //scope
            XElement scope = xmlModel.Element("Scope");
            int scopeId = 0;
            string scopeName = scope.Attribute("Name").Value;
            Int32.TryParse(scope.Attribute("ClassificationId").Value, out scopeId);
            model.Scope = new EquationScope( (EquationScopeClassification)scopeId, scopeName);

            //type
            XElement type = xmlModel.Element("Type");
            int typeId = 0;
            string typeName = type.Attribute("Name").Value;
            Int32.TryParse(type.Attribute("ClassificationId").Value, out typeId);
            model.Type = new EquationType((EquationTypeClassification)typeId, typeName);

            return model;
        }

        public bool ContainsComment(BasicComment comment, bool compareReferences = false)
        {
            if (compareReferences)
            {
                for (int i = 0; i < m_comments.Count; i++)
                {
                    if (object.ReferenceEquals(m_comments[i], comment))
                    {
                        return true;
                    }
                }

                // Didn't find a matching reference
                return false;
            }

            // Coming here means that we want to do a value comparison (not reference)
            foreach (BasicComment bc in m_comments)
            {
                if (bc.Equals(comment))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks all the comments in this model to see if there's any one with matching comment text
        /// </summary>
        public bool ContainsComment(string commentText)
        {
            foreach (BasicComment bc in m_comments)
            {
                if (commentText.Equals(bc.CommentText))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #endregion

        #region private methods
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        
    }
}
