/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/
﻿using System.Collections.Generic;

namespace ChemProV.Validation.Rules
{
    /// <summary>
    /// Use this if you want to throw a collection of ValidationResult
    /// </summary>
    public class ValidationResultsException : System.Exception
    {
        private List<ValidationResult> validationResults;

        public List<ValidationResult> ValidationResults
        {
            get { return validationResults; }
        }

        public ValidationResultsException(List<ValidationResult> validationResults)
            : base()
        {
            this.validationResults = validationResults;
        }
    }

    /// <summary>
    /// Use this when you just want to throw one ValidationResult
    /// </summary>
    public class ValidationResultException : System.Exception
    {
        private ValidationResult validationResult;

        public ValidationResult ValidationResult
        {
            get { return validationResult; }
        }

        public ValidationResultException(ValidationResult validationResult)
            : base()
        {
            this.validationResult = validationResult;
        }
    }
}