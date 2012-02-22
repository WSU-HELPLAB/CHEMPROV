/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/
﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ChemProV.PFD.EquationEditor.Tokens;

namespace ChemProV.PFD.EquationEditor
{
    public class EquationData : IDisposable
    {
        #region Fields

        private EquationControl equationReference;

        private ObservableCollection<IEquationToken> equationTokens;

        private ObservableCollection<IEquationToken> equationTokensDeepCopy;

        private Tuple<List<string>, List<string>> variableNames;

        private bool isValid = false;

        #endregion Fields

        /// <summary>
        /// Item 1 is the LHS of the equation.  Item 2 is the RHS of the equation
        /// </summary>
        public Tuple<List<string>, List<string>> VariableNames
        {
            get
            {
                return variableNames;
            }
        }

        public ObservableCollection<IEquationToken> EquationTokensDeepCopy
        {
            get
            {
                return equationTokensDeepCopy;
            }
        }

        public EquationControl EquationReference
        {
            get { return equationReference; }
        }

        public bool IsValid
        {
            get { return isValid; }
        }

        public ComboBoxEquationTypeItem Type
        {
            get
            {
                return equationReference.SelectedItem;
            }
        }

        public ObservableCollection<IEquationToken> EquationTokens
        {
            get { return equationReference.EquationTokens; }
        }

        public EquationData(EquationControl equation)
        {
            equationReference = equation;
            equationTokens = equationReference.EquationTokens;
            //equationTokensDeepCopy = deepCopy();
            variableNames = BuildVariableList(equationTokens);
            equation.EquationTokens.CollectionChanged += new NotifyCollectionChangedEventHandler(EquationTokens_CollectionChanged);
        }

        private void EquationTokens_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            equationTokensDeepCopy = deepCopy();
            variableNames = BuildVariableList(equationTokens);
        }

        private ObservableCollection<IEquationToken> deepCopy()
        {
            ObservableCollection<IEquationToken> equationDeepCopy = new ObservableCollection<IEquationToken>();

            foreach (IEquationToken token in equationTokens)
            {
                if (token is OperatorToken)
                {
                    equationDeepCopy.Add(new OperatorToken(token.Value));
                }
                else if (token is VariableToken)
                {
                    equationDeepCopy.Add(new VariableToken(token.Value));
                }
                else
                {
                    throw new Exception("Unknown token");
                }
            }

            return equationDeepCopy;
        }

        private Tuple<List<string>, List<string>> BuildVariableList(ObservableCollection<IEquationToken> equation)
        {
            List<string> firstlist = new List<string>();
            List<string> currentList = new List<string>();
            foreach (IEquationToken token in equation)
            {
                if (token is VariableToken)
                {
                    try
                    {
                        float.Parse(token.Value);
                        //it is a number so don't add to our list
                    }
                    catch
                    {
                        //not a number so must be a variable
                        currentList.Add(token.Value);
                    }
                }
                else if (token.Value == "=")
                {
                    firstlist = currentList;
                    currentList = new List<string>();
                }
            }

            if (firstlist.Count > 0 && currentList.Count > 0)
            {
                isValid = true;
            }

            return new Tuple<List<string>, List<string>>(firstlist, currentList);
        }

        public void Dispose()
        {
            //gotta stop listening to ourselves
            equationReference.EquationTokens.CollectionChanged -= new NotifyCollectionChangedEventHandler(EquationTokens_CollectionChanged);
        }
    }
}