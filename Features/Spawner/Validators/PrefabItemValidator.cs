using System;
using FluentValidation;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Validators;

public class PrefabItemValidator : AbstractValidator<PrefabItem>
{
    public PrefabItemValidator(IServiceProvider provider)
    {
        RuleFor(x => x.Id).NotEqual(default(Guid));
        RuleFor(x => x.Name).MinimumLength(5).NotEmpty();
        RuleFor(x => x.Folder).NotEmpty();
        RuleFor(x => x.Path).NotEmpty();
        RuleFor(x => x.AccelerationG).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Events)
            .SetValidator(events => new PrefabEventsValidator(events.Events));
    }

    public class PrefabEventsValidator : AbstractValidator<PrefabEvents>
    {
        public PrefabEventsValidator(PrefabEvents events)
        {
            
        }
    }
}