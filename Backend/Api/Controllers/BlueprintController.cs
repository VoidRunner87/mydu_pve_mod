using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json.Linq;
using NQ;
using Swashbuckle.AspNetCore.Annotations;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("bp")]
public partial class BlueprintController : Controller
{
    public class ImportBlueprintRequest
    {
        public string Folder { get; set; } = "imports";
        public string File { get; set; }
        public Vec3 Position { get; set; }
        public ulong? OwnerPlayerId { get; set; } = 2;
        public ulong? OwnerOrganizationId { get; set; }
        public string? Name { get; set; }
        public ulong? ParentId { get; set; }
    }

    [SwaggerOperation("Downloads a Blueprint from a Folder")]
    [Route("download/{folder}/{file}")]
    [HttpGet]
    public async Task<IActionResult> DownloadAsync(string folder, string file)
    {
        folder = OnlyBasicTextRegex()
            .Replace(folder, "");
        file = OnlyBasicTextRegex()
            .Replace(file, "");
        
        var dataFolderPath = NQutils.Config.Config.Instance.s3.override_base_path;
        
        var filePath = Path.Combine(dataFolderPath, folder, file);
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("File not found.");
        }
        
        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        
        return File(fileBytes, "application/octet-stream", file);
    }

    [SwaggerOperation("Uploads a plus blueprint to a Folder")]
    [Route("upload/{folder}")]
    [HttpPost]
    public async Task<IActionResult> UploadAsync(string folder, IFormFile? file)
    {
        if (file == null || file.Length == 0 || !file.FileName.EndsWith("json"))
        {
            return BadRequest("Invalid");
        }

        var dataFolderPath = NQutils.Config.Config.Instance.s3.override_base_path;

        var filePath = Path.Combine(dataFolderPath, folder, file.FileName);

        await using var readContentStream = file.OpenReadStream();
        using var sr = new StreamReader(readContentStream);
        var blueprintContents = await sr.ReadToEndAsync();
        var blueprintJToken = JObject.Parse(blueprintContents);

        if (blueprintJToken["fixtureheader"] == null)
        {
            return BadRequest("Not a correct blueprint type");
        }
        
        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        
        return Ok($"File {file.FileName} uploaded successfully");
    }
    
    [SwaggerOperation("Uploads a regular blueprint (and sanitizes it) to a Folder")]
    [Route("upload/sanitize/{folder}")]
    [HttpPost]
    public async Task<IActionResult> UploadSanitize2Async(string folder, IFormFile? file)
    {
        if (file == null || file.Length == 0 || !file.FileName.EndsWith("json"))
        {
            return BadRequest("Invalid");
        }

        var dataFolderPath = NQutils.Config.Config.Instance.s3.override_base_path;

        var filePath = Path.Combine(dataFolderPath, folder, file.FileName);

        await using var readContentStream = file.OpenReadStream();
        using var sr = new StreamReader(readContentStream);
        var blueprintContents = await sr.ReadToEndAsync();

        var blueprintSanitizerService = ModBase.ServiceProvider.GetRequiredService<IBlueprintSanitizerService>();
        var bytes = Encoding.UTF8.GetBytes(blueprintContents);
        var result = await blueprintSanitizerService.SanitizeAsync(
            ModBase.ServiceProvider.GetGameplayBank(),
            bytes,
            CancellationToken.None
        );

        if (!result.Success)
        {
            return BadRequest($"Failed to sanitize blueprint {result.Message}");
        }

        blueprintContents = Encoding.UTF8.GetString(result.BlueprintBytes);
        
        var blueprintJToken = JObject.Parse(blueprintContents);

        if (blueprintJToken["Model"] == null)
        {
            return BadRequest("Not a correct blueprint type");
        }
        
        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        
        return Ok($"File {file.FileName} uploaded successfully");
    }
    
    [SwaggerOperation("Uploads a plus blueprint (and sanitizes it) to a Folder")]
    [Route("upload/sanitize/plusbp/{folder}")]
    [HttpPost]
    public async Task<IActionResult> UploadSanitizeAsync(string folder, IFormFile? file)
    {
        if (file == null || file.Length == 0 || !file.FileName.EndsWith("json"))
        {
            return BadRequest("Invalid");
        }

        var dataFolderPath = NQutils.Config.Config.Instance.s3.override_base_path;

        var filePath = Path.Combine(dataFolderPath, folder, file.FileName);

        await using var readContentStream = file.OpenReadStream();
        using var sr = new StreamReader(readContentStream);
        var blueprintContents = await sr.ReadToEndAsync();

        var blueprintSanitizerService = ModBase.ServiceProvider.GetRequiredService<IBlueprintSanitizerService>();
        var bytes = Encoding.UTF8.GetBytes(blueprintContents);
        var result = await blueprintSanitizerService.SanitizePlusAsync(
            ModBase.ServiceProvider.GetGameplayBank(),
            bytes,
            CancellationToken.None
        );

        if (!result.Success)
        {
            return BadRequest($"Failed to sanitize blueprint {result.Message}");
        }

        blueprintContents = Encoding.UTF8.GetString(result.BlueprintBytes);
        
        var blueprintJToken = JObject.Parse(blueprintContents);

        if (blueprintJToken["fixtureheader"] == null)
        {
            return BadRequest("Not a correct blueprint type");
        }
        
        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        
        return Ok($"File {file.FileName} uploaded successfully");
    }

    [SwaggerOperation("Spawns a Blueprint")]
    [HttpPost]
    [Route("import")]
    public async Task<IActionResult> SpawnAsync([FromBody] ImportBlueprintRequest request)
    {
        var provider = ModBase.ServiceProvider;
        var spawnerService = provider.GetRequiredService<IBlueprintSpawnerService>();
        
        var constructId = await spawnerService.SpawnAsync(
            new SpawnArgs
            {
                File = request.File,
                Folder = request.Folder,
                Name = request.Name ?? $"W-{TimePoint.Now().networkTime}",
                Position = request.Position,
                OwnerEntityId = new EntityId{playerId = request.OwnerPlayerId ?? 0, organizationId = request.OwnerOrganizationId ?? 0},
            }
        );

        return Ok(new { constructId });
    }

    [GeneratedRegex("[^a0-z9_\\.]")]
    private static partial Regex OnlyBasicTextRegex();
}
