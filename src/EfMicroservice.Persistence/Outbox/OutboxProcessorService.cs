using EfMicroservice.Persistence.Contexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EfMicroservice.Persistence.Outbox
{
    public class OutboxProcessorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxProcessorService> _logger;
        private readonly OutboxOptions _options;

        public OutboxProcessorService(
            IServiceScopeFactory scopeFactory,
            ILogger<OutboxProcessorService> logger,
            IOptions<OutboxOptions> options)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox processor started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingMessagesAsync(stoppingToken);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Outbox processor encountered an error");
                }

                await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
            }
        }

        private async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var publishEndpoint = scope.ServiceProvider.GetService<IPublishEndpoint>();

            var messages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedAt == null && m.RetryCount < _options.MaxRetries)
                .OrderBy(m => m.OccurredAt)
                .Take(_options.BatchSize)
                .ToListAsync(cancellationToken);

            if (!messages.Any()) return;

            _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

            foreach (var message in messages)
            {
                try
                {
                    if (publishEndpoint != null)
                    {
                        var messageType = ResolveType(message.Type);
                        if (messageType != null)
                        {
                            var payload = JsonSerializer.Deserialize(message.Content, messageType);
                            await publishEndpoint.Publish(payload, messageType, cancellationToken);
                        }
                        else
                        {
                            _logger.LogWarning("Could not resolve type {TypeName} for outbox message {MessageId}",
                                message.Type, message.Id);
                        }
                    }

                    message.ProcessedAt = DateTimeOffset.UtcNow;
                    _logger.LogDebug("Published outbox message {MessageId} of type {Type}",
                        message.Id, message.Type);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process outbox message {MessageId} of type {Type} (RetryCount: {RetryCount})",
                        message.Id, message.Type, message.RetryCount);
                    message.RetryCount++;
                    message.Error = ex.Message;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static Type ResolveType(string fullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(fullName))
                .FirstOrDefault(t => t != null);
        }
    }
}
