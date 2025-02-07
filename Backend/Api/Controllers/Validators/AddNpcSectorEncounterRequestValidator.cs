using FluentValidation;

namespace Mod.DynamicEncounters.Api.Controllers.Validators;

public class AddNpcSectorEncounterRequestValidator : 
    AbstractValidator<SectorEncountersController.AddNpcSectorEncounterRequest>
{
    public AddNpcSectorEncounterRequestValidator()
    {
        RuleFor(x => x.NpcScript).NotEmpty();
        RuleFor(x => x.POIScript).NotEmpty();
    }
}