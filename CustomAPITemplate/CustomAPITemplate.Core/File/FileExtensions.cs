using CustomAPITemplate.Core.Extensions;
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
        var response = new Response<string>();
        if (file == null || file.Length <= 0)
        {
            response.Results.Add(new()
            {
                Message = "File is null",
                Severity = Severity.Error
            });
            return response;
        }

        var fileExtension = file.FileName.Split('.')?.Last()?.ToLowerEN();
        if (string.IsNullOrWhiteSpace(fileExtension) || !fileHelper.AllowedFileExtensions.Contains(fileExtension))
        {
            response.Results.Add(new()
            {
                Message = $"File extension is not allowed! Allowed extensions are: {string.Join(",", fileHelper.AllowedFileExtensions)}",
                Severity = Severity.Error
            });
            return response;
        }

        var randomFileName = Guid.NewGuid().ToString();
        var filePath = $"Files\\{fileHelper.FolderName}\\{randomFileName}.{fileExtension}";
        var fullPath = $"{fileHelper.WwwRootPath}\\{filePath}";
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        using (var fileStream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream, token);
        }
        response.Value = filePath;
        return response;
    }
}