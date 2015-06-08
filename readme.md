# Azure Service Bus Send/Receive

A bare bones C# solution that demonstrates how you can use Azure Queues and Topics with SessionIDs for correlation. This allows a consumer of the service(s) to send a message, and then wait for a response (for that specific message) on another queue or topic.

The solution contains two console applications:
1. **Sender** - sends a message to an Azure Queue, and waits for a response with a specific SessionID on an Azure Topic.
2. **Receiver** - the "server" that receives the message on the queue, "processes" it and sends a response with the same SessionID to the Azure Topic.