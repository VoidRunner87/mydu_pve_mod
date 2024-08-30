using System.Threading.Tasks;
using BotLib.BotClient;

namespace Mod.DynamicEncounters.Features.TaskQueue.Interfaces;

public interface ITaskQueueService
{
    Task ProcessQueueMessages(Client client);
}