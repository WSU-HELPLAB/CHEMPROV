using System.Windows.Controls;

namespace ChemProV.PFD.EquationEditor
{
    public enum EquationClassification
    {
        VariableDefinition,
        Energy,
        Overall,
        Compound,
        Element,
    }

    public class ComboBoxEquationTypeItem : ComboBoxItem
    {
        private EquationClassification classification;

        public EquationClassification Classification
        {
            get { return classification; }
            set { classification = value; }
        }

        public ComboBoxEquationTypeItem(EquationClassification type, string name = null)
            : base()
        {
            classification = type;

            if (name == null)
            {
                //name =  GetDisplayString(type) unless it returned null then it sets it to empty string
                name = GetDisplayString(type) ?? "";
            }
            this.Content = name;
        }

        public static string GetDisplayString(EquationClassification type)
        {
            switch (type)
            {
                case EquationClassification.VariableDefinition: return "Variable Definition";
                case EquationClassification.Energy: return "Energy";
                case EquationClassification.Overall: return "Overall";
                case EquationClassification.Compound: return null;
                case EquationClassification.Element: return null;
                default: return null;
            }
        }
    }
}