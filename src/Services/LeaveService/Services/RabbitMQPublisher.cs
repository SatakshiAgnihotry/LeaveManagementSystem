using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace LeaveService.Services;

public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
{
    private readonly IConnection? _connection;
    private readonly IModel? _channel;
    private readonly ILogger<RabbitMQPublisher> _logger;
   private static readonly ActivitySource ActivitySource = new("rabbitmq.publisher");
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    private const string ExchangeName = "leave.management.exchange";
    private const string QueueName = "leave.notifications";

    public RabbitMQPublisher(IConfiguration configuration, ILogger<RabbitMQPublisher> logger)
    {
        _logger = logger;
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"] ?? "rabbitmq",
                Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = configuration["RabbitMQ:Username"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange — durable means it survives RabbitMQ restart
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, durable: true);

            // Declare queue — durable means messages survive restart
            _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);

            // Bind queue to exchange for each routing key
            _channel.QueueBind(QueueName, ExchangeName, "leave.applied");
            _channel.QueueBind(QueueName, ExchangeName, "leave.approved");
            _channel.QueueBind(QueueName, ExchangeName, "leave.rejected");

            _logger.LogInformation("RabbitMQ connection established.");
        }
        catch (Exception ex)
        {
            // Graceful degradation — app still works without RabbitMQ
            // Notifications just won't be sent
            _logger.LogWarning(ex, "Could not connect to RabbitMQ. Events will not be published.");
        }
    }

    public void PublishLeaveApplied(LeaveEventMessage message) =>
        Publish(message with { EventType = "LeaveApplied" }, "leave.applied");

    public void PublishLeaveApproved(LeaveEventMessage message) =>
        Publish(message with { EventType = "LeaveApproved" }, "leave.approved");

    public void PublishLeaveRejected(LeaveEventMessage message) =>
        Publish(message with { EventType = "LeaveRejected" }, "leave.rejected");

    private void Publish(LeaveEventMessage message, string routingKey)
{
    // Create producer span
    using var activity = ActivitySource.StartActivity("RabbitMQ.Publish", ActivityKind.Producer);

    activity?.SetTag("messaging.system", "rabbitmq");
    activity?.SetTag("messaging.destination", ExchangeName);
    activity?.SetTag("messaging.routing_key", routingKey);
    activity?.SetTag("messaging.protocol", "AMQP");
    activity?.SetTag("leave.request_id", message.LeaveRequestId);

    if (_channel == null || !_channel.IsOpen)
    {
        _logger.LogWarning("RabbitMQ channel not available. Skipping publish for {RoutingKey}.", routingKey);
        activity?.SetStatus(ActivityStatusCode.Error, "Channel not available");
        return;
    }

    var json = JsonSerializer.Serialize(message);
    var body = Encoding.UTF8.GetBytes(json);

    var properties = _channel.CreateBasicProperties();
    properties.ContentType = "application/json";
    properties.DeliveryMode = 2; // Persistent
    properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

    // Inject trace context into headers
    properties.Headers ??= new Dictionary<string, object>();
    Propagator.Inject(
        new PropagationContext(activity?.Context ?? Activity.Current?.Context ?? default, Baggage.Current),
        properties.Headers,
        (headers, key, value) => headers[key] = value
    );

    activity?.SetTag("messaging.message_payload_size_bytes", body.Length);

    _channel.BasicPublish(ExchangeName, routingKey, properties, body);
    _logger.LogInformation("Published {RoutingKey} event for leave {LeaveRequestId}.", routingKey, message.LeaveRequestId);

    activity?.SetStatus(ActivityStatusCode.Ok);
}


    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}