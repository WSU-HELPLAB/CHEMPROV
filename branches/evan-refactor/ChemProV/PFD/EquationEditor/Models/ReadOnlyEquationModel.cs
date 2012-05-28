namespace ChemProV.PFD.EquationEditor.Models
{
    /// <summary>
    /// Provides a read-only wrapper for an equation model
    /// </summary>
    public class ReadOnlyEquationModel
    {
        private EquationModel m_model;

        public ReadOnlyEquationModel(EquationModel model)
        {
            m_model = model;
        }

        public string Annotation
        {
            get
            {
                return m_model.Annotation;
            }
        }

        public string Equation
        {
            get
            {
                return m_model.Equation;
            }
        }

        public int Id
        {
            get
            {
                return m_model.Id;
            }
        }

        public EquationScope Scope
        {
            get
            {
                return m_model.Scope;
            }
        }

        public EquationType Type
        {
            get
            {
                return m_model.Type;
            }
        }
    }
}
