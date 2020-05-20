namespace DocumentManager.Api.Helpers
{
    /// <summary>
    /// Interface for files validation
    /// </summary>
    public interface IFileValidator
    {
        /// <summary>
        /// File extension
        /// </summary>
        string Extension { get; }
        
        /// <summary>
        /// Maximum file size allowed (in bytes)
        /// </summary>
        long MaxFileSizeInBytes { get; }
        
        /// <summary>
        /// Check if file extension is valid
        /// </summary>
        /// <param name="fileName">File name (with extension)</param>
        /// <returns></returns>
        bool IsValidExtension(string fileName);
        
        /// <summary>
        /// Check if file size is greater than allowed
        /// </summary>
        /// <param name="size">File size (in bytes)</param>
        /// <returns></returns>
        bool IsExceedsMaxSize(long size);
    }
}
