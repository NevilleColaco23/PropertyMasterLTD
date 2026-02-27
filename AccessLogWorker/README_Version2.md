# How to Debug and Test RabbitMQ Integration

This guide helps verify that your API can publish `AccessLogEvent` messages to RabbitMQ, and that your AccessLogWorker is successfully consuming these events.

---

## 1. Start RabbitMQ

If you use Docker Compose (recommended):

```sh
docker-compose up -d
```

You should be able to access RabbitMQ Management UI at: [http://localhost:15672](http://localhost:15672)  
Default credentials: `guest` / `guest`

---

## 2. Start the AccessLogWorker (Consumer)

Ensure your `MONGODB_URI` and `RabbitMq` options are set correctly in your environment or `appsettings.json`.

```sh
dotnet run --project AccessLogWorker
```

You should see log output indicating the worker is connected to RabbitMQ and ready.

---

## 3. Start the API (Producer)

Ensure `appsettings.json` in your API project points to the correct RabbitMQ host & credentials.

```sh
dotnet run --project testAngularAPIDocker
```

---

## 4. Send a Test Message from API

Using `curl` (substitute port if needed):

```sh
curl -X POST http://localhost:5000/api/accesslog \
  -H "Content-Type: application/json" \
  -d '{"Method":"GET","Path":"/test","StatusCode":200,"DurationMs":15,"UserId":"test","Username":"user","TraceId":"abcde","ClientIp":"127.0.0.1","UserAgent":"curl/7.0"}'
```

Or use Postman to POST the same payload.

---

## 5. Verify in RabbitMQ Management UI

1. Go to [http://localhost:15672](http://localhost:15672), login.
2. Click on "Queues", and find `accesslog.queue`
3. If messages were published but not yet consumed, you’ll see "Ready" count > 0.
4. If your worker is running, the "Ready" count should quickly drop to 0, and "Get messages" will show recent messages.

---

## 6. Verify the Worker Receives the Message

Check your worker console output for log lines like:

```
Received log event: { Method = GET, Path = /test, StatusCode = 200, ... }
```

If you see this, your integration is working!

---

## 7. Troubleshooting

- **Can’t connect to RabbitMQ?**
  - Is RabbitMQ running? `docker ps`
  - Are the host/port/user/pass set correctly in your configs?
  - Can you access the management UI?

- **API returns error on POST?**
  - Check API logs for connection errors.
  - Verify exchange/routingKey in `RabbitMqOptions`.

- **Messages stuck in queue?**
  - Worker not running or can’t connect? Check Worker logs for exceptions.

- **Worker receives nothing?**
  - Make sure `exchange`, `queue`, and `routingKey` all match between API and Worker configs.
  - Confirm your message contract is correct and not being deserialized as null.

---

## 8. References

- [RabbitMQ Management UI Docs](https://www.rabbitmq.com/management.html)
- [RabbitMQ Docker image](https://hub.docker.com/_/rabbitmq/)

---

*Happy debugging!*