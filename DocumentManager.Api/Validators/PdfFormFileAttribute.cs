using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace DocumentManager.Api.Validators
{
    /// <summary>
    /// Attribute for validation Pdf files obtained from the Form
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class PdfFormFileAttribute : ValidationAttribute
    {
        private const string _extension = ".pdf";
        private readonly long _maxSizeInBytes;

        public PdfFormFileAttribute(long maxSizeInMb)
        {
            _maxSizeInBytes = maxSizeInMb * 1024 * 1024;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile formFile)
            {
                if (formFile.FileName == null || !_extension.Equals(Path.GetExtension(formFile.FileName), StringComparison.InvariantCultureIgnoreCase))
                    return new ValidationResult($"File should have '{_extension}' extension");

                if (formFile.Length > _maxSizeInBytes)
                    return new ValidationResult($"File size should not be greater than '{_maxSizeInBytes}' bytes");
            }

            return ValidationResult.Success;
        }
    }
}
