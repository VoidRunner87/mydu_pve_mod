using Mod.DynamicEncounters.Tests.Stubs.Weapons;

namespace Mod.DynamicEncounters.Tests.Features.Common.Data;

[TestFixture]
public class WeaponItemTests
{
    [Test]
    public void Should_Retrieve_Single_Weapon_Shot_Wait_Time()
    {
        var weapon = WeaponItemStubFactory.RareLargeDefenseRailgun();
        var ammoItems = weapon.GetAmmoItems().ToList();

        var shotWaitTimePerGunSingle = weapon.GetShotWaitTimePerGun(ammoItems.First(), 1);

        Assert.That(shotWaitTimePerGunSingle, Is.GreaterThan(17d));
        Assert.That(shotWaitTimePerGunSingle, Is.LessThanOrEqualTo(17.21d));
        
        var shotWaitTimePerGunTwo = weapon.GetShotWaitTimePerGun(ammoItems.First(), 2);
        
        Assert.That(shotWaitTimePerGunTwo, Is.GreaterThan(8d));
        Assert.That(shotWaitTimePerGunTwo, Is.LessThanOrEqualTo(8.61d));
    }
}