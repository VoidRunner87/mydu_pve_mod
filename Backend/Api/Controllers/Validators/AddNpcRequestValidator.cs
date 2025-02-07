using FluentValidation;

namespace Mod.DynamicEncounters.Api.Controllers.Validators;

public class AddNpcRequestValidator : AbstractValidator<NpcController.AddNpcRequest>
{
    public AddNpcRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.BlueprintPath).NotEmpty();
        RuleFor(x => x.Folder).NotEmpty();
        RuleFor(x => x.ConstructName).NotEmpty();
        RuleFor(x => x.AmmoItems).NotEmpty();
        RuleFor(x => x.WeaponItems).NotEmpty();
    }
}