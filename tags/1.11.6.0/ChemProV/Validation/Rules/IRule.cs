/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
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