using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Backend;
using Backend.AWS;
using Backend.Database;
using Backend.Fixture;
using Backend.Fixture.Construct;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json.Linq;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using NQutils.Sql;
using Orleans;
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

    [SwaggerOperation("Uploads a blueprint to a Folder")]
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
