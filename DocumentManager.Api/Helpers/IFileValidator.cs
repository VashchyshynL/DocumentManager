using Microsoft.AspNetCore.Http;
namespace DocumentManager.Api.Helpers
{
    public interface IFileValidator
    {
        string Extension { get; }
        long MaxFileSizeInBytes { get; }
        bool IsValidExtension(string fileName);
        bool IsExceedsMaxSize(long size);
    }
}
