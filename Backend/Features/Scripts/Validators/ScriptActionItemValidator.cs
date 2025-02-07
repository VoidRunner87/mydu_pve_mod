using System;
using FluentValidation;
using Mod.DynamicEncounters.Features.Scripts.Actions;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;

namespace Mod.DynamicEncounters.Features.Scripts.Validators;

public class ScriptActionItemValidator : AbstractValidator<ScriptActionItem>
{
    public ScriptActionItemValidator(IServiceProvider provider, int depth = 0)
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x).Must(BeValidQuantityRange);
        RuleFor(x => x.MinQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Type)
            .SetValidator(new ScriptActionTypeValidator(provider))
            .When(x => !string.IsNullOrEmpty(x.Type) && depth >= 0)
            .WithMessage("Action Type is required when not on a root (first action on a script) action");
        RuleFor(x => x.Events)
            .SetValidator(item => new ScriptActionEventsValidator(provider));
        RuleFor(x => depth)
            .LessThan(3)
            .WithMessage("Too many scripts inside scripts");
        RuleFor(x => x.Prefab)
            .NotEmpty()
            .When(TypeIsSpawnAction);
        RuleFor(x => x.Area)
            .NotNull()
            .When(TypeIsSpawnAction);
        RuleFor(x => x.Message)
            .NotEmpty()
            .When(TypeIsSendMessage);
        RuleFor(x => x.Area)
            .NotNull()
            .When(TypeIsSpawnAction);
    }

    private static bool TypeIsSpawnAction(ScriptActionItem item) => item.Type == SpawnScriptAction.ActionName;
    private static bool TypeIsSendMessage(ScriptActionItem item) => item.Type == SendDirectMessageAction.ActionName;

    private static bool BeValidQuantityRange(ScriptActionItem actionItem)
    {
        return actionItem.MinQuantity <= actionItem.MaxQuantity;
    }
}