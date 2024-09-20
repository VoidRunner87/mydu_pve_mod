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
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json.Linq;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using NQutils.Sql;
using Orleans;

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

    [Route("upload/{folder}")]
    [HttpPost]
    public async Task<IActionResult> UploadAsync(string folder, [FromForm] IFormFile? file)
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

    [HttpPut]
    [Route("import")]
    public async Task<IActionResult> ImportAsync([FromBody] ImportBlueprintRequest request)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();
        var logger = provider.CreateLogger<BlueprintController>();

        var s3 = provider.GetRequiredService<IS3>();

        var settings = NQutils.Config.Config.Instance.wrecks;
        settings.override_path = request.Folder;

        var constructJson = await s3.Get(
            settings,
            request.File
        );

        using var source = FixtureSource.FromStringContent(
            constructJson,
            FixtureKind.Construct
        );

        var fixture = ConstructFixture.FromSource(source);
        fixture.parentId = request.ParentId;
        if (!string.IsNullOrEmpty(request.Name))
        {
            fixture.header.prettyName = request.Name;
        }

        fixture.ownerId = new EntityId
            { playerId = request.OwnerPlayerId ?? 0, organizationId = request.OwnerOrganizationId ?? 0 };
        fixture.position = request.Position;
        fixture.isUntargetable = false;
        fixture.isNPC = false;
        fixture.serverProperties.dynamicFixture = false;
        fixture.serverProperties.isDynamicWreck = false;
        fixture.header.constructIdHint = null;

        var constructId = (await ConstructFixtureImport.Import(
            fixture,
            provider.GetRequiredService<IUserContent>(),
            provider.GetRequiredService<ISql>(),
            provider.GetRequiredService<IVoxelImporter>(),
            provider.GetRequiredService<IGameplayBank>(),
            provider.GetRequiredService<IRDMSStorage>(),
            provider.GetRequiredService<IPlanetList>()
        ))[0];

        logger.LogInformation(
            "Spawned Construct [{Name}]({Id}) at ::pos{{0,0,{Pos}}}", 
            request.Name, 
            constructId,
            request.Position
        );

        var clusterClient = provider.GetRequiredService<IClusterClient>();
        await clusterClient.GetConstructParentingGrain().ReloadConstruct(constructId);

        var constructElementsGrain = orleans.GetConstructElementsGrain(constructId);
        var shields = await constructElementsGrain.GetElementsOfType<ShieldGeneratorUnit>();

        var constructGrain = orleans.GetConstructGrain(constructId);
        if (shields.Count > 0)
        {
            var sql = provider.GetRequiredService<ISql>();
            await sql.SetShieldEnabled(constructId, true);

            await constructGrain.UpdateConstructInfo(new ConstructInfoUpdate
            {
                shieldState = new ShieldState
                {
                    hasShield = true,
                    isActive = true,
                    shieldHpRatio = 1
                }
            });
        }

        return Ok(new { constructId });
    }

    [GeneratedRegex("[^a0-z9_\\.]")]
    private static partial Regex OnlyBasicTextRegex();
}