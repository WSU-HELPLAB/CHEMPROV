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
using ChemProV.PFD.EquationEditor.Models;
using System.Linq;

namespace ChemProV.PFD.EquationEditor
{
    public class EquationViewModel : INotifyPropertyChanged
    {
        #region public members
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public IList<IPfdElement> RelatedElements { get; set; }
        #endregion

        #region private members
        private static int _id = 0;
        private EquationScope _scope = new EquationScope();
        private EquationType _type = new EquationType();
        private string _equation = "";
        private ObservableCollection<EquationScope> _scopeOptions = new ObservableCollection<EquationScope>();
        private ObservableCollection<EquationType> _typeOptions = new ObservableCollection<EquationType>();
        #endregion

        #region properties
        public int Id { get; private set; }
        public ObservableCollection<EquationScope> ScopeOptions
        {
            get
            {
                return _scopeOptions;
            }
            set
            {
                ObservableCollection<EquationScope> newScopes = value;

                //remove any invalid scopes
                foreach (EquationScope scope in _scopeOptions.ToArray())
                {
                    if (!newScopes.Contains(scope))
                    {
                        _scopeOptions.Remove(scope);
                    }
                }

                //add in new scopes
                foreach (EquationScope scope in newScopes)
                {
                    if (!_scopeOptions.Contains(scope))
                    {
                        _scopeOptions.Add(scope);
                    }
                }
                OnPropertyChanged("ScopeOptions");
            }
        }
        public ObservableCollection<EquationType> TypeOptions
        {
            get
            {
                return _typeOptions;
            }
            set
            {
                ObservableCollection<EquationType> newTypes = value;
                
                //remove any invalid types
                foreach (EquationType type in _typeOptions.ToArray())
                {
                    if (!newTypes.Contains(type))
                    {
                        _typeOptions.Remove(type);
                    }
                }

                //add in new types
                foreach (EquationType type in newTypes)
                {
                    if (!_typeOptions.Contains(type))
                    {
                        _typeOptions.Add(type);
                    }
                }
                OnPropertyChanged("TypeOptions");
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
        public EquationViewModel()
        {
            _id++;
            Id = _id;
            RelatedElements = new List<IPfdElement>();
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
