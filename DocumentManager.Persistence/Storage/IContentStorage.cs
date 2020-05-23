using System.IO;
using System.Threading.Tasks;

namespace DocumentManager.Persistence.Storage
{
    /// <summary>
    /// Interface for interaction with content storage
    /// </summary>
    public interface IContentStorage
    {
        /// <summary>
        /// Save file to storage
        /// </summary>
        /// <param name="stream">File's stream</param>
        /// <param name="fileName">File name</param>
        /// <returns>Uri location of the document in storage</returns>
        Task<string> SaveFile(Stream stream, string fileName);

        /// <summary>
        /// Delete file from storage
        /// </summary>
        /// <param name="fileName">File name</param>
        Task DeleteFile(string fileName);
    }
}
