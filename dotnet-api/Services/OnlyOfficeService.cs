using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EduPlatform.Api.Services;

public class OnlyOfficeConfig
{
    public string ServerUrl { get; set; } = "";
    public string PublicUrl { get; set; } = "";
    public string JwtSecret { get; set; } = "";
    public string CallbackBaseUrl { get; set; } = "";
}

public class OnlyOfficeService
{
    private readonly OnlyOfficeConfig _config;

    public OnlyOfficeService(IConfiguration configuration)
    {
        _config = configuration.GetSection("OnlyOffice").Get<OnlyOfficeConfig>()
                  ?? throw new Exception("OnlyOffice config missing");

        // Allow override from environment variables
        var envSecret = configuration["ONLYOFFICE__JwtSecret"];
        if (!string.IsNullOrEmpty(envSecret))
            _config.JwtSecret = envSecret;
    }

    // ── Build the config object that Vue passes to DocsAPI.DocEditor ──
    public object BuildViewerConfig(string fileId, string fileName, string fileUrl)
    {
        var ext = Path.GetExtension(fileName).TrimStart('.').ToLower();

        var docConfig = new
        {
            documentType = GetDocumentType(ext),
            document = new
            {
                fileType  = ext,
                key       = fileId,           // unique key — change when file changes
                title     = fileName,
                url       = fileUrl,          // ONLYOFFICE fetches the file from here
                permissions = new
                {
                    edit     = false,         // view-only for students
                    download = false,         // prevent download if desired
                    print    = false
                }
            },
            editorConfig = new
            {
                mode = "view",
                lang = "ar",                  // Arabic UI — change to "en-US" if needed
                customization = new
                {
                    autosave    = false,
                    forcesave   = false,
                    compactHeader = true,
                    toolbarNoTabs = true,
                    hideRightMenu = true
                }
            }
        };

        // Sign the whole config as JWT so ONLYOFFICE validates it
        var token = SignPayload(docConfig);
        return new { config = docConfig, token };
    }

    // ── Sign any payload as a JWT with the shared secret ──
    // OnlyOffice verifies by checking each top-level config key as a JWT claim,
    // so we must spread the payload object into claims — not wrap it as a string.
    public string SignPayload(object payload)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.JwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Deserialize to Dictionary so every top-level key becomes a JWT claim
        var json   = JsonSerializer.Serialize(payload);
        var claims = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;

        var descriptor = new SecurityTokenDescriptor
        {
            Claims             = claims,
            Expires            = DateTime.UtcNow.AddHours(2),
            SigningCredentials = creds
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    // ── Map file extension to ONLYOFFICE document type ──
    private static string GetDocumentType(string ext) => ext switch
    {
        "ppt" or "pptx" or "pptm" or "ppsx" or "odp" => "presentation",
        "xls" or "xlsx" or "xlsm" or "ods" or "csv"  => "spreadsheet",
        "doc" or "docx" or "docm" or "odt" or "rtf"
             or "txt" or "pdf"                        => "word",
        _                                             => "word"
    };
}
