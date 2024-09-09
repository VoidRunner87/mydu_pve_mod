using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Common.Data;

namespace Mod.DynamicEncounters.Features.Common.Interfaces;

public interface IErrorRepository
{
    Task AddAsync(ErrorItem item);
}