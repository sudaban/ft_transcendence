using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Backend.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Backend.Infrastructure.Services;

public class FileUploadService : IFileUploadService
{
    private readonly string _uploadDirectory;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".mp4" };
    private readonly long _maxFileSize = 10 * 1024 * 1024; // 10 MB

    public FileUploadService(IConfiguration configuration)
    {
        _uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        
        if (!Directory.Exists(_uploadDirectory))
        {
            Directory.CreateDirectory(_uploadDirectory);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        if (fileStream == null || fileStream.Length == 0)
        {
            throw new ArgumentException("File stream is empty", nameof(fileStream));
        }

        if (fileStream.Length > _maxFileSize)
        {
            throw new InvalidOperationException($"File size exceeds the limit of 10 MB.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Unsupported file type.");
        }

        var unique_file_name = $"{Guid.NewGuid()}{extension}";
        var file_path = Path.Combine(_uploadDirectory, unique_file_name);

        using (var stream = new FileStream(file_path, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream);
        }

        return $"/uploads/{unique_file_name}";
    }

    public bool DeleteFile(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return false;

        var file_name = Path.GetFileName(fileUrl);
        var file_path = Path.Combine(_uploadDirectory, file_name);

        if (File.Exists(file_path))
        {
            File.Delete(file_path);
            return true;
        }

        return false;
    }
}
