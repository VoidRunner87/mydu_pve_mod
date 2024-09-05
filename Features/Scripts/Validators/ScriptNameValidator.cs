using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Validators;

public class ScriptNameValidator : AbstractValidator<string>
{
    private readonly IServiceProvider _provider;

    public ScriptNameValidator(IServiceProvider provider)
    {
        _provider = provider;

        RuleFor(x => x).MustAsync(Exist)
            .WithMessage(actionName => $"Script named '{actionName}' is invalid");
    }

    private async Task<bool> Exist(string actionName, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        var repository = _provider.GetRequiredService<IScriptActionItemRepository>();

        return await repository.ActionExistAsync(actionName);
    }
}