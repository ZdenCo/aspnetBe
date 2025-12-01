using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProperAuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly string _tempFolder;
    private readonly ILogger<UploadController> _logger;

    public UploadController(ILogger<UploadController> logger)
    {
        _tempFolder = Path.Combine(Path.GetTempPath(), "uploads");

        if (!Directory.Exists(_tempFolder))
        {
            Directory.CreateDirectory(_tempFolder);
        }
    }
    [HttpGet("upload")]
    [Authorize]
    public async Task<IActionResult> UploadFiles(List<IFormFile> files)
    {
        _logger.LogInformation("UploadFiles called with {FileCount} files", files.Count);
        if (files.Count == 0)
        {
            return BadRequest("No files uploaded.");
        }

        var user = HttpContext.Items["User"] as User;

        if (user.GuidId == null)
        {
            return Unauthorized("User not authenticated.");
        }

        var userFilePath = Path.Combine(_tempFolder, user.GuidId.ToString());
        var projectFilePath = Path.Combine(userFilePath, $"{Guid.NewGuid()}");
        _logger.LogInformation("Saving files to {ProjectFilePath}", projectFilePath);

        Directory.CreateDirectory(projectFilePath);

        foreach (var file in files)
        {
            var relativePath = Path.GetFileName(file.FileName);
            _logger.LogInformation("Processing file {FileName} with relative path {RelativePath}", file.FileName, relativePath);
            relativePath = relativePath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
            _logger.LogInformation("Normalized relative path: {RelativePath}", relativePath);
            var fullPath = Path.Combine(projectFilePath, relativePath);
            var directoryPath = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            _logger.LogInformation("Saved file to {FullPath}", fullPath);
        }

        return Ok(new { message = "Files uploaded successfully." });
    }
}
