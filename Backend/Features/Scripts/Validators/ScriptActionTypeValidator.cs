using System;
using System.Linq;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Validators;

public class ScriptActionTypeValidator : AbstractValidator<string>
{
    private readonly IServiceProvider _provider;
    
    public ScriptActionTypeValidator(IServiceProvider provider)
    {
        _provider = provider;
        
        RuleFor(x => x).Must(Exist)
            .WithMessage(type => $"Script Action Type '{type}' is invalid");
    }
    
    private bool Exist(string type)
    {
        var factory = _provider.GetRequiredService<IScriptActionFactory>();

        return factory.GetAllActions().ToHashSet().Contains(type);
    }
}