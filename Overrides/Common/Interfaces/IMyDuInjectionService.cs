using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Mod.DynamicEncounters.Overrides.Common.Interfaces;

public interface IMyDuInjectionService
{
    Task InjectJs(ulong playerId, string code);
    Task InjectCss(ulong playerId, string code);
    Task UploadJson(ulong playerId, string key, JToken data);
    Task UploadJson(ulong playerId, string key, object data);
    Task SetContext(ulong playerId, object data);
}