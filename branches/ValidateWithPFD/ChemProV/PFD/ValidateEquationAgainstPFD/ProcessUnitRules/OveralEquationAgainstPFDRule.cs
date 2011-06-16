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
using System.Collections.Generic;

using ChemProV.PFD.EquationEditor.Tokens;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.PFD.Streams;
using ChemProV.PFD.ProcessUnits;
using ChemProV.Validation.Rules;
using ChemProV.PFD.ValidateEquationAgainstPFD;
using ChemProV.UI.DrawingCanvas;
using ChemProV.Validation.Rules.Adapters.Table;

namespace ChemProV.PFD.ValidateEquationAgainstPFD.ProcessUnitRules
{
    public enum Relation
    {
        AdjacentIn,
        AdjacentOut,
        InOrderOpposite,
        OutOfOrderOpposite,
        Unknown
    }

    public class OveralEquationAgainstPFDRule : IValidateEquationWithPFD
    {
        private IList<IEquationToken> lhs;

        private IList<IEquationToken> rhs;

        public IList<IEquationToken> Equation
        {
            get
            {
                //gotta rebuild the equation
                List<IEquationToken> list; 
                list = new List<IEquationToken>();
                list.AddRange(rhs);
                list.AddRange(lhs);
                return list;
            }
            set
            {
                int equalSignLocation = findEqualsSign(value);

                if (equalSignLocation != -1)
                {
                    lhs = new List<IEquationToken>(value);
                    for (int i = equalSignLocation; i < value.Count; i++)
                    {
                        lhs.RemoveAt(equalSignLocation);
                    }

                    rhs = new List<IEquationToken>(value);

                    //the plus one because we want to remove the equalSign as well
                    for (int i = 0; i < equalSignLocation + 1; i++)
                    {
                        rhs.RemoveAt(0);
                    }
                }
            }
        }

        private int findEqualsSign(IList<IEquationToken> equation)
        {
            int i = 0;
            foreach (IEquationToken token in equation)
            {
                if (token.Value == "=")
                {
                    return i;
                }
                i++;
            }
            //returns -1 on fail
            return (-1);
        }

        private IList<IPropertiesWindow> propertiesWindows;

        public IList<IPropertiesWindow> PropertiesWindows
        {
            get { return propertiesWindows; }
            set { propertiesWindows = value; }
        }

        public ValidationResult check()
        {
            IEquationToken firstTablelLabelLHS = null;
            IEquationToken firstTableLabelRHS = null;

            IPropertiesWindow firstWindowLHS = null;
            IPropertiesWindow firstWindowRHS = null;

            IProcessUnit ipu;

            int i = 0;
            if (lhs != null && rhs != null && lhs.Count > 0 && rhs.Count > 0)
            {
                while (firstTablelLabelLHS == null && i < lhs.Count)
                {
                    if (lhs[i] is VariableToken)
                    {
                        if (isTableLabel(lhs[i].Value, out firstWindowLHS))
                        {
                            firstTablelLabelLHS = lhs[i];
                        }
                    }
                    i++;
                }

                i = 0;
                while (firstTableLabelRHS == null && i < rhs.Count)
                {
                    if (rhs[i] is VariableToken)
                    {
                        if (isTableLabel(rhs[i].Value, out firstWindowRHS))
                        {
                            firstTableLabelRHS = rhs[i];
                        }
                    }
                    i++;
                }

                if (firstTablelLabelLHS == null || firstTableLabelRHS == null)
                {
                    //return ERROR
                }

                Relation relation = determineRelations(firstWindowLHS, firstWindowRHS);

                if (relation == Relation.Unknown || relation == Relation.AdjacentIn || relation == Relation.AdjacentOut)
                {
                    new ValidationResult(null, "");
                }
                else
                {
                    //so they are either InOrderOpposite or OutOfOrderOpposite
                    //now we gotta check to see if all the overalls are being added together
                    if (relation == Relation.InOrderOpposite)
                    {
                        ipu = (firstWindowLHS as IPropertiesWindow).ParentStream.Destination;
                        AllIn(ipu);
                    }
                    else
                    {
                        ipu = (firstWindowRHS as IPropertiesWindow).ParentStream.Destination;
                        AllOut(ipu);
                    }
                }
            }

            return ValidationResult.Empty;
        }

        bool AllIn(IProcessUnit ipu)
        {
            throw new NotImplementedException();
            /*foreach (IStream stream in ipu.IncomingStreams)
            {
                
            }*/
        }

        bool AllOut(IProcessUnit ipu)
        {
            throw new NotImplementedException();
        }

        bool isTableLabel(string label, out IPropertiesWindow tableFoundIn)
        {
            foreach(IPropertiesWindow window in propertiesWindows)
            {
                ITableAdapter tableAdapter = TableAdapterFactory.CreateTableAdapter(window);
                int i = 0, rows = tableAdapter.GetRowCount();
                while (i < rows)
                {
                    if (label == tableAdapter.GetLabelAtRow(i))
                    {
                        tableFoundIn = window;
                        return true;
                    }
                    i++;
                }
            }
            tableFoundIn = null;
            return false;
        }

        Relation determineRelations(IPropertiesWindow label, IPropertiesWindow label2)
        {
            foreach(IStream stream in label.ParentStream.Destination.OutgoingStreams)
            {
                if (stream.Table == label2)
                {
                    return Relation.InOrderOpposite;
                }
            }
            foreach (IStream stream in label.ParentStream.Source.IncomingStreams)
            {
                if (stream.Table == label2)
                {
                    return Relation.OutOfOrderOpposite;
                }
            }
            foreach (IStream stream in label.ParentStream.Destination.IncomingStreams)
            {
                if (stream.Table == label2)
                {
                    return Relation.AdjacentIn;
                }
            }
            foreach (IStream stream in label.ParentStream.Source.OutgoingStreams)
            {
                if (stream.Table == label2)
                {
                    return Relation.AdjacentOut;
                }
            }
            return Relation.Unknown;
        }
    }
}
