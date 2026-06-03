using System.Text;
using System.Text.Json;
using NotificationService.Data;
using NotificationService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace NotificationService.Services;

public class RabbitMQConsumerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    private const string ExchangeName = "leave.management.exchange";
    private const string QueueName = "leave.notifications";

    public RabbitMQConsumerService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Retry logic with exponential backoff for RabbitMQ connection
        int retryCount = 0;
        const int maxRetries = 10;
        int delayMs = 2000;

        while (retryCount < maxRetries && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _configuration["RabbitMQ:Host"] ?? "rabbitmq",
                    Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                    UserName = _configuration["RabbitMQ:Username"] ?? "guest",
                    Password = _configuration["RabbitMQ:Password"] ?? "guest"
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declare same exchange + queue as publisher — safe to call multiple times
                _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, durable: true);
                _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(QueueName, ExchangeName, "leave.applied");
                _channel.QueueBind(QueueName, ExchangeName, "leave.approved");
                _channel.QueueBind(QueueName, ExchangeName, "leave.rejected");

                // Process one message at a time — don't overwhelm the service
                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);
                        var message = JsonSerializer.Deserialize<LeaveEventMessage>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (message != null)
                            await ProcessMessageAsync(message, ea.RoutingKey);

                        // Acknowledge — tell RabbitMQ "I processed this, remove it from queue"
                        _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error processing message");
                        // Reject and discard — don't requeue to avoid infinite retry loop
                        _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                    }
                };

                _channel.BasicConsume(QueueName, autoAck: false, consumer: consumer);
                Log.Information("RabbitMQ consumer started. Listening on queue: {Queue}", QueueName);

                // Connection successful — keep running until app shuts down
                while (!stoppingToken.IsCancellationRequested)
                    await Task.Delay(1000, stoppingToken);
                return;
            }
            catch (Exception ex)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    Log.Error(ex, "Failed to connect to RabbitMQ after {RetryCount} attempts. Notifications will not be received.", maxRetries);
                    return;
                }
                
                Log.Warning(ex, "Failed to connect to RabbitMQ (Attempt {RetryCount}/{MaxRetries}). Retrying in {DelayMs}ms...", retryCount, maxRetries, delayMs);
                await Task.Delay(delayMs, stoppingToken);
                delayMs = Math.Min(delayMs * 2, 30000); // Exponential backoff, max 30s
            }
        }
    }

    private async Task ProcessMessageAsync(LeaveEventMessage message, string routingKey)
    {
        // Use a scope because AppDbContext is Scoped but this is a Singleton BackgroundService
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

        switch (routingKey)
        {
            case "leave.applied":
                // Notify employee — their request was submitted
                await repository.AddNotificationAsync(new Notification
                {
                    UserId = message.EmployeeId,
                    Type = NotificationType.LeaveApplied,
                    Message = $"Your {message.LeaveType} leave ({message.StartDate:yyyy-MM-dd} to {message.EndDate:yyyy-MM-dd}) has been submitted successfully."
                });
                // Notify manager — action needed
                await repository.AddNotificationAsync(new Notification
                {
                    UserId = message.ManagerId,
                    Type = NotificationType.LeaveApplied,
                    Message = $"{message.EmployeeName} applied for {message.LeaveType} leave ({message.StartDate:yyyy-MM-dd} to {message.EndDate:yyyy-MM-dd}). Reason: {message.Reason}"
                });
                break;

            case "leave.approved":
                await repository.AddNotificationAsync(new Notification
                {
                    UserId = message.EmployeeId,
                    Type = NotificationType.LeaveApproved,
                    Message = $"Your {message.LeaveType} leave ({message.StartDate:yyyy-MM-dd} to {message.EndDate:yyyy-MM-dd}) has been approved." +
                              (string.IsNullOrEmpty(message.Comments) ? "" : $" Comments: {message.Comments}")
                });
                break;

            case "leave.rejected":
                await repository.AddNotificationAsync(new Notification
                {
                    UserId = message.EmployeeId,
                    Type = NotificationType.LeaveRejected,
                    Message = $"Your {message.LeaveType} leave ({message.StartDate:yyyy-MM-dd} to {message.EndDate:yyyy-MM-dd}) has been rejected." +
                              (string.IsNullOrEmpty(message.Comments) ? "" : $" Reason: {message.Comments}")
                });
                break;
        }

        Log.Information("Processed {RoutingKey} event for employee {EmployeeId}", routingKey, message.EmployeeId);
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}