using System;
using FluentValidation;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;

namespace Mod.DynamicEncounters.Features.Scripts.Validators;

public class ScriptActionEventsValidator : AbstractValidator<ScriptActionEvents>
{
    public ScriptActionEventsValidator(IServiceProvider provider, int depth = 0)
    {
        RuleForEach(x => x.OnLoad)
            .SetValidator(new ScriptActionItemValidator(provider, depth + 1));
        RuleForEach(x => x.OnSectorEnter)
            .SetValidator(new ScriptActionItemValidator(provider, depth + 1));
    }
}