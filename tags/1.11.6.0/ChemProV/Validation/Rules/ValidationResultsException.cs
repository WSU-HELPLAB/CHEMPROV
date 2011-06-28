using System.Collections.Generic;

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