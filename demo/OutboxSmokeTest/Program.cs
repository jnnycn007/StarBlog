using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StarBlog.Data;
using StarBlog.Data.Models;
using StarBlog.Web.Services.OutboxServices;

var services = new ServiceCollection();

services.AddLogging(builder => {
    builder.ClearProviders();
    builder.AddSimpleConsole(options => { options.SingleLine = true; });
    builder.SetMinimumLevel(LogLevel.Information);
});

var dbPath = Path.Combine(AppContext.BaseDirectory, "outbox-smoke.db");
if (File.Exists(dbPath)) File.Delete(dbPath);

var fsql = FreeSqlFactory.Create($"Data Source={dbPath};");
services.AddSingleton<IFreeSql>(fsql);
services.AddFreeRepository();

services.Configure<OutboxOptions>(options => {
    options.BatchSize = 10;
    options.PollInterval = TimeSpan.FromMilliseconds(10);
    options.LeaseDuration = TimeSpan.FromSeconds(30);
    options.DefaultMaxAttempts = 2;
});

services.AddScoped<OutboxService>();
services.AddScoped<OutboxProcessor>();
services.AddScoped<IOutboxHandler, NoopOutboxHandler>();

var serviceProvider = services.BuildServiceProvider();

using (var scope = serviceProvider.CreateScope()) {
    var outboxService = scope.ServiceProvider.GetRequiredService<OutboxService>();
    var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
    var outboxRepo = scope.ServiceProvider.GetRequiredService<IBaseRepository<OutboxMessage>>();

    var messageId = await outboxService.EnqueueAsync("noop", "{}");
    var allBefore = await outboxRepo.Select.ToListAsync();
    Console.WriteLine($"Outbox rows before processing: {allBefore.Count}");
    foreach (var row in allBefore) {
        Console.WriteLine($"- #{row.Id} type={row.Type} status={row.Status} next={row.NextAttemptAt:o} lockedUntil={(row.LockedUntil?.ToString("o") ?? "null")}");
    }

    var processed = await processor.ProcessOnceAsync("smoke-test", CancellationToken.None);
    Console.WriteLine($"Processed: {processed}");

    var message = await outboxRepo.Where(a => a.Id == messageId).FirstAsync();
    if (message == null) throw new Exception("Outbox message not found");
    if (message.Status != OutboxStatus.Succeeded) {
        throw new Exception($"Outbox smoke test failed: status={message.Status}, lastError={message.LastError}");
    }
}

Console.WriteLine("Outbox smoke test succeeded.");

file sealed class NoopOutboxHandler : IOutboxHandler {
    public string Type => "noop";

    public Task HandleAsync(OutboxMessage message, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}
