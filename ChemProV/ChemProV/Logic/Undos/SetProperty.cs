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
using System.Reflection;

namespace ChemProV.Logic.Undos
{
    /// <summary>
    /// Represents and undo/redo action that can set a property on an object.
    /// </summary>
    public class SetProperty : IUndoRedoAction
    {
        private object m_obj;
        
        private string m_propName;
        
        private object m_propVal;

        public SetProperty(object objectToSetPropertyOn, string propertyName, object propertyValue)
        {
            m_obj = objectToSetPropertyOn;
            m_propName = propertyName;
            m_propVal = propertyValue;
        }

        public IUndoRedoAction Execute(Workspace sender)
        {
            // Get the current value of the property so that we can create the opposite action
            PropertyInfo pi = m_obj.GetType().GetProperty(m_propName);
            object currentValue = pi.GetValue(m_obj, null);
            SetProperty opposite = new SetProperty(m_obj, m_propName, currentValue);

            // Set the property
            pi.SetValue(m_obj, m_propVal, null);

            return opposite;
        }
    }
}
