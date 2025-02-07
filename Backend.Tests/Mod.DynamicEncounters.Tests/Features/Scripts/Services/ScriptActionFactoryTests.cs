using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common.Interfaces;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Features.TaskQueue.Interfaces;
using Mod.DynamicEncounters.Tests.Resources;
using NQ;
using NSubstitute;
using Orleans;

namespace Mod.DynamicEncounters.Tests.Features.Scripts.Services;

[TestFixture]
public class ScriptActionFactoryTests
{
    private IServiceCollection _serviceCollection;

    [SetUp]
    public void Setup()
    {
        _serviceCollection = new ServiceCollection();
        _serviceCollection.AddLogging();
        _serviceCollection.AddSingleton(Substitute.For<IFeatureReaderService>());

        var randomProvider = Substitute.For<IRandomProvider>();
        randomProvider.GetRandom().Returns(new Random());

        _serviceCollection.AddSingleton(randomProvider);
        _serviceCollection.AddSingleton(Substitute.For<IClusterClient>());
    }

    [Test]
    public void Tag_As_Active_Should_Read_DelaySeconds_Property_From_Script()
    {
        var constructService = Substitute.For<IConstructService>();
        constructService.GetConstructInfoAsync(Arg.Any<ulong>())
            .Returns(new ConstructInfoOutcome(true, new ConstructInfo()));

        _serviceCollection.AddSingleton(constructService);

        var constructHandleRepository = Substitute.For<IConstructHandleRepository>();
        constructHandleRepository.FindTagInSectorAsync(Arg.Any<Vec3>(), Arg.Any<string>())
            .Returns(new List<ConstructHandleItem>
            {
                new() { ConstructId = 1 }
            });

        _serviceCollection.AddSingleton(constructHandleRepository);

        var taskQueueService = Substitute.For<ITaskQueueService>();
        taskQueueService.EnqueueScript(Arg.Any<ScriptActionItem>(), Arg.Any<DateTime>());

        _serviceCollection.AddSingleton(taskQueueService);

        var provider = _serviceCollection.BuildServiceProvider();
        ModBase.ServiceProvider = provider;

        var factory = new ScriptActionFactory();

        var scriptActionItem = ResourceRepository.TagSectorAsActiveScript;

        var action = factory.Create(scriptActionItem);

        Assert.DoesNotThrowAsync(async () =>
        {
            var result = await action.ExecuteAsync(
                new ScriptContext(
                    provider,
                    1,
                    [],
                    new Vec3(),
                    null
                )
            );

            Assert.That(result.Success);
        });

        taskQueueService.Received(1)
            .EnqueueScript(
                Arg.Any<ScriptActionItem>(),
                Arg.Is<DateTime>(x => x > DateTime.UtcNow + TimeSpan.FromSeconds(990)
                )
            );
    }
}