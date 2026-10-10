using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Backend.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Backend.Application.Exceptions;

namespace Backend.Infrastructure.Services;

public class FileUploadService : IFileUploadService
{
    private readonly string _uploadDirectory;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png"};
    private readonly long _maxFileSize = 10 * 1024 * 1024; 

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
            throw new BadRequestException($"File size exceeds the limit of 10 MB.");
        }

        var allowedMimeTypes = new[] { "image/jpeg", "image/png" };
        if (string.IsNullOrEmpty(contentType) || !allowedMimeTypes.Contains(contentType.ToLowerInvariant()))
        {
            throw new BadRequestException("Unsupported Content-Type.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            throw new BadRequestException("Unsupported file type.");
        }

        var headerBytes = new byte[8];
        var originalPosition = fileStream.Position;
        int bytesRead = await fileStream.ReadAsync(headerBytes, 0, headerBytes.Length);
        fileStream.Position = originalPosition;

        bool isValidImage = false;
        if (bytesRead >= 3 && headerBytes[0] == 0xFF && headerBytes[1] == 0xD8 && headerBytes[2] == 0xFF)
        {
            isValidImage = extension == ".jpg" || extension == ".jpeg";
        }
        else if (bytesRead >= 8 && headerBytes[0] == 0x89 && headerBytes[1] == 0x50 && headerBytes[2] == 0x4E && headerBytes[3] == 0x47)
        {
            isValidImage = extension == ".png";
        }

        if (!isValidImage)
        {
            throw new BadRequestException("File content does not match allowed image formats.");
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
