using System.Threading;
using System.Threading.Tasks;
using Backend;
using Mod.DynamicEncounters.Features.Common.Data;

namespace Mod.DynamicEncounters.Features.Common.Interfaces
{
    public interface IBlueprintSanitizerService
    {
        Task<BlueprintSanitationResult> SanitizeAsync(IGameplayBank bank, byte[] blueprintBytes, CancellationToken cancellationToken);
    }
}