using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.TaskQueue.Data;
using Mod.DynamicEncounters.Features.TaskQueue.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mod.DynamicEncounters.Features.TaskQueue.Repository;

public class TaskQueueRepository(IServiceProvider provider) : ITaskQueueRepository
{
    private IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task AddAsync(TaskQueueItem item)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            INSERT INTO public.mod_task_queue (id, command, delivery_at, data, status)
            VALUES (@id, @command, @delivery_at, @data, @status)
            """,
            new
            {
                id = item.Id,
                command = item.Command,
                delivery_at = item.DeliveryAt,
                data = JsonConvert.SerializeObject(item.Data),
                status = "scheduled"
            }
        );
    }

    public async Task<IEnumerable<TaskQueueItem>> FindNextAsync(int quantity)
    {
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<DbRow>(
            $"""
             SELECT * FROM public.mod_task_queue ORDER BY created_at ASC LIMIT {quantity} 
             """)).ToList();

        return result.Select(MapToModel);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync("DELETE FROM public.mod_task_queue WHERE id = @id", new { id });
    }

    private TaskQueueItem MapToModel(DbRow row)
    {
        return new TaskQueueItem
        {
            Id = row.id,
            Command = row.command,
            DeliveryAt = row.delivery_at,
            Data = JToken.Parse(row.data),
            Status = row.status
        };
    }

    private struct DbRow
    {
        public Guid id { get; set; }
        public string command { get; set; }
        public DateTime delivery_at { get; set; }
        public string data { get; set; }
        public string status { get; set; }
    }
}