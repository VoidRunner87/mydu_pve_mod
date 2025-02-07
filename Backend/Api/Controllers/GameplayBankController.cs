using Microsoft.AspNetCore.Mvc;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("bank")]
public class GameplayBankController : Controller
{
    [HttpGet]
    [Route("{elementTypeName}")]
    public IActionResult Get(string elementTypeName)
    {
        var provider = ModBase.ServiceProvider;
        var bank = provider.GetGameplayBank();

        return Ok(bank.GetDefinition(elementTypeName));
    }
}