using System;

namespace DocumentManager.Api.Helpers
{
    public class PdfFileValidator : IFileValidator
    {
        public string Extension => ".pdf";

        public long MaxFileSizeInBytes => 5242880;

        public bool IsValidExtension(string fileName)
        {
            return fileName != null && fileName.EndsWith(Extension, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool IsExceedsMaxSize(long size)
        {
            return size > MaxFileSizeInBytes;
        }
    }
}
