using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using EduPlatform.Api.Services;

namespace EduPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly OnlyOfficeService _onlyOffice;
    private readonly string _storagePath;
    private readonly string _apiBaseUrl;

    public FilesController(IConfiguration config, OnlyOfficeService onlyOffice)
    {
        _config      = config;
        _onlyOffice  = onlyOffice;
        _storagePath = config["Files:StoragePath"] ?? "/app/storage";
        _apiBaseUrl  = config["OnlyOffice:CallbackBaseUrl"] ?? "http://localhost:5000";
    }

    // ── POST /api/files/upload ─────────────────────────────────────────
    // Instructor uploads a course file
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        var ext = Path.GetExtension(file.FileName).ToLower();
        var allowed = _config.GetSection("Files:AllowedExtensions").Get<string[]>()
                      ?? new[] { ".pptx", ".docx", ".xlsx", ".pdf" };

        if (!allowed.Contains(ext))
            return BadRequest($"File type {ext} not allowed");

        // Generate a unique ID for this file
        var fileId   = Guid.NewGuid().ToString("N");
        var fileName = $"{fileId}{ext}";
        var filePath = Path.Combine(_storagePath, fileName);

        Directory.CreateDirectory(_storagePath);
        using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream);

        return Ok(new
        {
            fileId,
            originalName = file.FileName,
            fileName,
            size = file.Length
        });
    }

    // ── GET /api/files/{fileId}/viewer-config ─────────────────────────
    // Vue calls this to get the ONLYOFFICE config + signed JWT
    [HttpGet("{fileId}/viewer-config")]
    public IActionResult GetViewerConfig(string fileId, [FromQuery] string originalName)
    {
        // Find the file on disk (any extension)
        var files = Directory.GetFiles(_storagePath, $"{fileId}.*");
        if (files.Length == 0)
            return NotFound("File not found");

        var filePath = files[0];
        var ext      = Path.GetExtension(filePath);
        var fileName = string.IsNullOrEmpty(originalName)
                       ? Path.GetFileName(filePath)
                       : originalName;

        // URL that ONLYOFFICE container will use to fetch the file
        // Must be reachable from the ONLYOFFICE container — uses internal Docker network
        var fileUrl = $"{_apiBaseUrl}/api/files/{fileId}/download";

        var viewerConfig = _onlyOffice.BuildViewerConfig(fileId, fileName, fileUrl);
        return Ok(viewerConfig);
    }

    // ── GET /api/files/{fileId}/download ──────────────────────────────
    // Called by ONLYOFFICE container to fetch the original file
    [HttpGet("{fileId}/download")]
    public IActionResult Download(string fileId)
    {
        var files = Directory.GetFiles(_storagePath, $"{fileId}.*");
        if (files.Length == 0)
            return NotFound();

        var filePath = files[0];
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(filePath, out var contentType))
            contentType = "application/octet-stream";

        var stream = System.IO.File.OpenRead(filePath);
        return File(stream, contentType, Path.GetFileName(filePath));
    }

    // ── GET /api/files ─────────────────────────────────────────────────
    // List all uploaded files
    [HttpGet]
    public IActionResult List()
    {
        if (!Directory.Exists(_storagePath))
            return Ok(new List<object>());

        var files = Directory.GetFiles(_storagePath)
            .Select(f => new
            {
                fileId       = Path.GetFileNameWithoutExtension(f),
                fileName     = Path.GetFileName(f),
                extension    = Path.GetExtension(f),
                sizeBytes    = new FileInfo(f).Length,
                uploadedAt   = new FileInfo(f).CreationTimeUtc
            })
            .OrderByDescending(f => f.uploadedAt)
            .ToList();

        return Ok(files);
    }
}
