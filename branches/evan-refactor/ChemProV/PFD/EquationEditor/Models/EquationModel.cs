using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using System.Xml.Serialization;

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
        private string _annotation = "";

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

        public string Annotation
        {
            get
            {
                return _annotation;
            }
            set
            {
                _annotation = value;
                OnPropertyChanged("Annotation");
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

            // TODO: Get rid of annotation and write all comments (but use the <Annotation> tag to maintain 
            // compatibility)
            writer.WriteStartElement("Annotation");
            writer.WriteString(Annotation);
            writer.WriteEndElement();
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
            model.Annotation = xmlModel.Element("Annotation").Value;

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
