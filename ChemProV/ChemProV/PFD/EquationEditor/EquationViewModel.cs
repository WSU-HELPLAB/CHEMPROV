using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace ChemProV.PFD.EquationEditor
{
    public class EquationViewModel : INotifyPropertyChanged
    {
        #region public members
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        #endregion

        #region private members
        private static int _id = 0;
        private string _scope;
        private string _type;
        private string _equation;
        #endregion

        #region properties
        public int Id { get; private set; }
        public ObservableCollection<string> ScopeOptions { get; set; }
        public ObservableCollection<string> TypeOptions { get; set; }
        public string Scope
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
        public string Type
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
        public EquationViewModel()
        {
            _id++;
            Id = _id;
            ScopeOptions = new ObservableCollection<string>();
            TypeOptions = new ObservableCollection<string>();
        }
        #endregion

        #region private methods
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
