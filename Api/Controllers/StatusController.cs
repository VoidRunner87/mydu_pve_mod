using Microsoft.AspNetCore.Mvc;

namespace Mod.DynamicEncounters.Api.Controllers;

public class StatusController : Controller
{
    public IActionResult Live()
    {
        return Ok("Alive");
    }
}