using System;

namespace Mod.DynamicEncounters.Features.Webhook.Data;

public class WebhookBody
{
    public string Id { get; set; }
    public string Name { get; set; }
    public object Data { get; set; }
    public ulong? PlayerId { get; set; }
    public DateTime RaisedAt { get; set; }
}