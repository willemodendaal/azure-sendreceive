using Microsoft.Azure;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("RECEIVER (server)");

            ListenForMessages();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey(true);
        }


        private static void ListenForMessages()
        {
            Console.WriteLine("Listening for messages on queue...");
            string queueName = "queue1";

            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ReceiverConnectionString");
            QueueClient Client =
              QueueClient.CreateFromConnectionString(connectionString, queueName);

            // Configure the callback options
            OnMessageOptions options = new OnMessageOptions();
            options.AutoComplete = false;
            options.AutoRenewTimeout = TimeSpan.FromMinutes(1);

            // Callback to handle received messages
            Client.OnMessage((message) =>
            {
                try
                {
                    Console.WriteLine("Got a message. Responding to client...");
                    SendResponseToTopic(message);

                    // Remove message from queue
                    message.Complete();
                }
                catch (Exception exc)
                {
                    // Indicates a problem, unlock message in queue
                    message.Abandon();
                    Console.WriteLine("* Error: " + exc.Message);
                }
            }, options);
        }

        private static void SendResponseToTopic(BrokeredMessage message)
        {
            
            string sessionId = message.ReplyToSessionId;
            string body = message.GetBody<string>();
            Console.WriteLine("Body: " + body + ", ReplyToSessionID: " + sessionId);

            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ResponseConnectionString");

            TopicClient Client =
                TopicClient.CreateFromConnectionString(connectionString, "response1");

            Client.Send(new BrokeredMessage("response_" + body)
                {
                   SessionId = sessionId //Ensure we are only responding to somebody waiting for this session message.
                });

            Console.WriteLine("Sent response with sessionId: " + sessionId);
        }
    }
}
