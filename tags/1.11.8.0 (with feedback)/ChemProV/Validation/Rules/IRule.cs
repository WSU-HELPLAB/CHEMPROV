/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.ObjectModel;

namespace ChemProV.Validation.Rules
{
    /// <summary>
    /// This is our interface for any rule.
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// Called to perform the underlying rule validation logic
        /// </summary>
        void CheckRule();

        /// <summary>
        /// A list of validation results associated with a particular rule
        /// </summary>
        ObservableCollection<ValidationResult> ValidationResults
        {
            get;
        }

        /// <summary>
        /// The target that the rule will be checking.
        /// </summary>
        Object Target
        {
            get;
            set;
        }
    }
}