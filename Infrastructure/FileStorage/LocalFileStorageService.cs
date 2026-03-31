using CompSci.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace CompSci.Infrastructure.FileStorage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly string _contentRootPath;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IWebHostEnvironment env, ILogger<LocalFileStorageService> logger)
    {
        _contentRootPath = env.ContentRootPath;
        _basePath = Path.Combine(env.ContentRootPath, "wwwroot", "uploads");
        _contentTypeProvider = new FileExtensionContentTypeProvider();
        _logger = logger;

        _logger.LogInformation("FileStorage initialized - ContentRootPath: {ContentRootPath}, BasePath: {BasePath}", _contentRootPath, _basePath);

        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder)
    {
        var folderPath = Path.Combine(_basePath, folder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);

        using var fileStreamOut = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamOut);

        return Path.Combine("uploads", folder, fileName);
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Task.FromResult(false);

        var fullPath = Path.Combine(_basePath, Path.GetFileName(filePath));

        if (filePath.Contains("pastquestions"))
            fullPath = Path.Combine(_basePath, "pastquestions", Path.GetFileName(filePath));
        else if (filePath.Contains("assignments"))
            fullPath = Path.Combine(_basePath, "assignments", Path.GetFileName(filePath));
        else if (filePath.Contains("notes"))
            fullPath = Path.Combine(_basePath, "notes", Path.GetFileName(filePath));

        var combinedPath = Path.Combine(
            _contentRootPath,
            "wwwroot",
            filePath.Replace("\\", "/")
        );

        if (File.Exists(combinedPath))
        {
            File.Delete(combinedPath);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public Task<(Stream FileStream, string ContentType)> GetFileAsync(string filePath)
    {
        var combinedPath = Path.Combine(
            _contentRootPath,
            "wwwroot",
            filePath.Replace("\\", "/")
        );

        if (!File.Exists(combinedPath))
            throw new FileNotFoundException($"File not found: {filePath}");

        _contentTypeProvider.TryGetContentType(combinedPath, out var contentType);
        contentType ??= "application/octet-stream";

        var fileStream = new FileStream(combinedPath, FileMode.Open, FileAccess.Read);
        return Task.FromResult(((Stream)fileStream, contentType));
    }

    public bool FileExists(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        var combinedPath = Path.Combine(
            _contentRootPath,
            "wwwroot",
            filePath.Replace("\\", "/")
        );

        var exists = File.Exists(combinedPath);
        if (!exists)
            _logger.LogWarning("File not found on disk. FilePath: {FilePath}, ResolvedPath: {ResolvedPath}", filePath, combinedPath);

        return exists;
    }
}
