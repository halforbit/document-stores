using Halforbit.ObjectTools.Validation;
using System;

namespace Halforbit.DocumentStores
{
    public class DocumentValidationException : Exception
    {
        public DocumentValidationException(ValidationErrors validationErrors)
        {
            ValidationErrors = validationErrors ?? ValidationErrors.Empty;
        }

        public override string Message => $"Validation error(s) occurred: {ValidationErrors}";

        public ValidationErrors ValidationErrors { get; }
    }
}
