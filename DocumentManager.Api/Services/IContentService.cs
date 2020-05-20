using System.IO;
using System.Threading.Tasks;

namespace DocumentManager.Api.Services
{
    public interface IContentService
    {
        Task<string> SaveFile(Stream stream, string fileName);
        Task DeleteFile(string fileName);
    }
}
