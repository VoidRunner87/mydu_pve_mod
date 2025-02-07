using System;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Data;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;
using NQutils.Def;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Services;

public class StasisTargetSkill(SkillItem skillItem) : BaseSkill(skillItem)
{
    private static double MagazineVolume => 1500D;
    private static double AmmoUnitVolume => 100D;
    private static long TotalAmmoCount => (long)(MagazineVolume / AmmoUnitVolume);
    public long CurrentAmmo { get; set; } = TotalAmmoCount;

    public required string? ItemTypeName { get; set; }
    public required TimeSpan Cooldown { get; set; }
    public TimeSpan ReloadCooldown { get; set; } = TimeSpan.FromSeconds(30);

    public override bool CanUse(BehaviorContext context)
    {
        return (!context.Effects.IsEffectActive<CooldownEffect>() ||
               !context.Effects.IsEffectActive<ReloadEffect>()) &&
               base.CanUse(context);
    }

    public override bool ShouldUse(BehaviorContext context)
    {
        return context.HasTargetConstruct() && base.ShouldUse(context);
    }

    public override async Task Use(BehaviorContext context)
    {
        if (!context.HasTargetConstruct()) return;

        var constructService = context.Provider.GetRequiredService<IConstructService>();
        var bank = context.Provider.GetGameplayBank();

        var speedConfig = bank.GetBaseObject<ConstructSpeedConfig>();
        var totalMass = await constructService.GetConstructTotalMass(context.GetTargetConstructId()!.Value);

        var stasis = bank.GetDefinition(ItemTypeName ?? "StasisWeaponSmall");

        if (stasis?.BaseObject is not StasisWeaponUnit stasisWeaponUnit)
        {
            return;
        }

        var maxRange = stasisWeaponUnit.RangeMax;
        var distance = context.TargetDistance;

        if (distance > maxRange)
        {
            return;
        }

        if (totalMass <= speedConfig.heavyConstructMass)
        {
            var num2 = (stasisWeaponUnit.RangeMin - stasisWeaponUnit.RangeMax) /
                       (1.0 - 1.0 / (stasisWeaponUnit.RangeCurvature + 1.0));
            maxRange = stasisWeaponUnit.RangeMin - num2 +
                       num2 / (stasisWeaponUnit.RangeCurvature * totalMass / speedConfig.heavyConstructMass + 1.0);
        }

        context.Effects.Activate<CooldownEffect>(Cooldown);

        CurrentAmmo--;

        if (CurrentAmmo <= 0)
        {
            CurrentAmmo = TotalAmmoCount;
            context.Effects.Activate<ReloadEffect>(ReloadCooldown);
            return;
        }

        if (distance > maxRange * 3.0)
        {
            // miss
            return;
        }

        var strength = Math.Pow(0.5, Math.Max(distance - maxRange, 0.0) / maxRange) * stasisWeaponUnit.effectStrength;
        var duration = stasisWeaponUnit.effectDuration;

        await constructService.ApplyStasisEffect(
            context.GetTargetConstructId()!.Value,
            strength,
            duration
        );
    }

    public static StasisTargetSkill Create(SkillItem item)
    {
        return new StasisTargetSkill(item)
        {
            Cooldown = TimeSpan.FromSeconds(item.CooldownSeconds),
            ItemTypeName = item.ItemTypeName
        };
    }

    public class CooldownEffect : IEffect;

    public class ReloadEffect : IEffect;
}