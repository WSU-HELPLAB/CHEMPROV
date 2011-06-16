/*
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
*/

using System;

namespace ChemProV.Validation.Rules
{
    /// <summary>
    /// This class stores the infomation need for a validation result, that is
    /// the target and the associated message
    /// </summary>
    public class ValidationResult
    {
        private object target;
        private string message;
        private static ValidationResult empty = new ValidationResult();

        /// <summary>
        /// This constructer takes a target and a message and sets them to its properties target and message
        /// </summary>
        /// <param name="target">the rule breaker</param>
        /// <param name="message">the feedback message of why the rule was broken</param>
        public ValidationResult(object target, string message)
        {
            Target = target;
            Message = message;
        }

        /// <summary>
        /// Private constructor used in the static ValidationResult.Empty property
        /// </summary>
        private ValidationResult()
        {
            target = null;
            message = null;
        }

        /// <summary>
        /// Returns an empty validation result
        /// </summary>
        public static ValidationResult Empty
        {
            get
            {
                return empty;
            }
        }

        /// <summary>
        /// Returns whether or not the object is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (target == null && message == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the validation result's target
        /// </summary>
        public Object Target
        {
            get
            {
                return target;
            }
            set
            {
                target = value;
            }
        }

        /// <summary>
        /// Gets or sets the message associated with the validation result
        /// </summary>
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
            }
        }
    }
}