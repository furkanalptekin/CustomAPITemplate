namespace CustomAPITemplate.Core;

public class FileHelper
{
    public required string WwwRootPath { get; init; }
    public required string FolderName { get; init; }
    public required string[] AllowedFileExtensions { get; init; }
}