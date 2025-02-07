using FluentValidation;

namespace Mod.DynamicEncounters.Api.Controllers.Validators;

public class AddWreckSectorEncounterRequestValidator : 
    AbstractValidator<SectorEncountersController.AddWreckSectorEncounterRequest>
{
    public AddWreckSectorEncounterRequestValidator()
    {
        RuleFor(x => x.WreckScript).NotEmpty();
    }
}