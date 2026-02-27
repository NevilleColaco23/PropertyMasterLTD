# Messaging.Shared

This library contains shared contracts and configuration options for messaging with RabbitMQ.

- `AccessLogEvent` — Standard event contract for access log events sent over RabbitMQ.
- `RabbitMqOptions` — Options class for RabbitMQ configuration.
- `IRabbitMqPublisher` — Interface for publishing messages to RabbitMQ.

Reference this library from both API and worker projects to ensure message contract consistency.