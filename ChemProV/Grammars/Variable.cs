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
using ChemProV.Logic.Equations;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.Logic;

namespace ChemProV.Grammars
{
    public enum VariableSigns { Positive = 0, Negative };
    public class Variable
    {
        private static int _id = 0;
        private int _localId = 0;

        #region properties
        public EquationModel Model { get; set; }
        public IStreamData Data { get; set; }
        public string Name { get; set; }
        public VariableSigns Sign { get; set; }
        public bool IsPercent { get; set; }
        
        public int Id
        {
            get
            {
                if (_localId == 0)
                {
                    _localId = _id;
                    _id++;
                }
                return _localId;
            }
        }
        #endregion

        public Variable()
        {
            Sign = VariableSigns.Positive;
        }

        public Variable(string name)
        {
            Name = name;
            Sign = VariableSigns.Positive;
        }

    }
}
