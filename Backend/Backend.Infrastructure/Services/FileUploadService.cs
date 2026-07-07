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
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5 MB

    public FileUploadService(IConfiguration configuration)
    {
        // "Uploads" is a folder under wwwroot in the API project
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
            throw new InvalidOperationException($"File size exceeds the limit of 5 MB.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Unsupported file type.");
        }

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_uploadDirectory, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream);
        }

        // Return relative path to access via HTTP
        return $"/uploads/{uniqueFileName}";
    }

    public bool DeleteFile(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return false;

        var fileName = Path.GetFileName(fileUrl);
        var filePath = Path.Combine(_uploadDirectory, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }

        return false;
    }
}
