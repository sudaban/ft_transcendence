using System.IO;
using System.Threading.Tasks;

namespace Backend.Application.Abstractions;

public interface IFileUploadService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    bool DeleteFile(string fileUrl);
}
