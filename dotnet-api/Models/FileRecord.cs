namespace EduPlatform.Api.Models;

public class FileRecord
{
    public string FileId       { get; set; } = "";
    public string FileName     { get; set; } = "";
    public string OriginalName { get; set; } = "";
    public string Extension    { get; set; } = "";
    public long   SizeBytes    { get; set; }
    public DateTime UploadedAt { get; set; }
}
