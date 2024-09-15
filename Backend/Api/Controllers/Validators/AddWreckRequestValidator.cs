using FluentValidation;

namespace Mod.DynamicEncounters.Api.Controllers.Validators;

public class AddWreckRequestValidator : AbstractValidator<WreckController.AddWreckRequest>
{
    public AddWreckRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.BlueprintPath).NotEmpty();
        RuleFor(x => x.Folder).NotEmpty();
        RuleFor(x => x.ConstructName).NotEmpty();
    }
}