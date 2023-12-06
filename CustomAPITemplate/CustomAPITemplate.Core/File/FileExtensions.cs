using Microsoft.AspNetCore.Http;

namespace CustomAPITemplate.Core;

public static class FileExtensions
{
    /// <summary>
    /// Copies a file with random name and returns file path
    /// </summary>
    /// <param name="file"></param>
    /// <param name="fileHelper"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<Response<string>> CopyFile(this IFormFile file, FileHelper fileHelper, CancellationToken token)
    {
        if (file == null || file.Length <= 0)
        {
            return Result.Error("File is null");
        }

        var fileExtension = file.FileName.Split('.')?.Last()?.ToLowerEN();
        if (string.IsNullOrWhiteSpace(fileExtension) || !fileHelper.AllowedFileExtensions.Contains(fileExtension))
        {
            return Result.Error($"File extension is not allowed! Allowed extensions are: {string.Join(",", fileHelper.AllowedFileExtensions)}");
        }

        var filePath = Path.Combine("Files", fileHelper.FolderName, $"{Guid.NewGuid()}.{fileExtension}");
        var fullPath = Path.Combine(fileHelper.WwwRootPath, filePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        using (var fileStream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream, token);
        }

        return filePath;
    }
}