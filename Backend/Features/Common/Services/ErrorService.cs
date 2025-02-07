using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class ErrorService(IServiceProvider provider) : IErrorService
{
    private readonly IErrorRepository _errorRepository = provider.GetRequiredService<IErrorRepository>();
    
    public Task AddAsync(ErrorItem errorItem)
    {
        return _errorRepository.AddAsync(errorItem);
    }
}