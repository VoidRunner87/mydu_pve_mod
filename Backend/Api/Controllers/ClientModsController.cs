using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("clientmod")]
public class ClientModsController : Controller
{
    [Route("upload")]
    [HttpPost]
    public async Task<IActionResult> UploadAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0 || !file.FileName.EndsWith("zip"))
        {
            return BadRequest("Invalid");
        }

        var dataFolderPath = NQutils.Config.Config.Instance.s3.override_base_path;
        var clientModsPath = Path.Combine(dataFolderPath, "clientmods");
        var filePath = Path.Combine(clientModsPath, file.FileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(fileStream);

        var manifestFilePath = Path.Combine(clientModsPath, "manifest.json");

        var manifestJson = await System.IO.File.ReadAllTextAsync(manifestFilePath);
        var manifest = JsonConvert.DeserializeObject<ManifestData>(manifestJson);
        if (manifest == null)
        {
            manifest = new ManifestData();
        }
        manifest.Mods.Add(file.FileName.Replace(".zip", ""));

        await using var manifestFileStream = new FileStream(manifestFilePath, FileMode.Create);
        await using var streamWriter = new StreamWriter(manifestFileStream);
        await streamWriter.WriteAsync(JsonConvert.SerializeObject(manifest, Formatting.Indented));
        
        return Ok($"File {file.FileName} uploaded successfully");
    }

    [Route("{manifestName}")]
    [HttpDelete]
    public async Task<IActionResult> DeleteManifestItem(string manifestName)
    {
        var dataFolderPath = NQutils.Config.Config.Instance.s3.override_base_path;
        var clientModsPath = Path.Combine(dataFolderPath, "clientmods");
        var manifestFilePath = Path.Combine(clientModsPath, "manifest.json");
        
        var manifestJson = await System.IO.File.ReadAllTextAsync(manifestFilePath);
        var manifest = JsonConvert.DeserializeObject<ManifestData>(manifestJson);
        if (manifest == null)
        {
            manifest = new ManifestData();
        }
        manifest.Mods.Remove(manifestName);

        await using var manifestFileStream = new FileStream(manifestFilePath, FileMode.Create);
        await using var streamWriter = new StreamWriter(manifestFileStream);
        await streamWriter.WriteAsync(JsonConvert.SerializeObject(manifest, Formatting.Indented));

        System.IO.File.Delete(Path.Combine(clientModsPath, $"{manifestName}.zip"));
        
        return Ok();
    }

    public class ManifestData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mods")] public HashSet<string> Mods { get; set; } = [];
    }
}