using Microsoft.Azure;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sender
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SENDER (client)");
            Console.WriteLine("Press any key to send a message to the server...");
            Console.ReadKey(true);

            Random r = new Random();
            int messageId = r.Next(1000);

            SendMessage(messageId);
            ListenForResponse(messageId);

            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey(true);

        }

      
        private static void SendMessage(int messageId)
        {
            Console.WriteLine("Sending to the server...");
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.QueueConnectionString");
            string queueName = "queue1";

            QueueClient queueClient = QueueClient.CreateFromConnectionString(connectionString, queueName);

            string msg = "msg" + messageId;
            var brokered = new BrokeredMessage(msg)
                {
                    ReplyToSessionId = Convert.ToString(messageId) //Send and specify this message has a sessionID response.
                };
            queueClient.Send(brokered);

            Console.WriteLine("Sent " + msg);
        }

        private static void ListenForResponse(int messageId)
        {

            string subName = "SenderSub";
            Console.WriteLine("Listening for response from server... ");

            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.TopicConnectionString");

            SubscriptionClient Client =
                SubscriptionClient.CreateFromConnectionString
                        (connectionString, "response1", subName);
            var session = Client.AcceptMessageSession(Convert.ToString(messageId), new TimeSpan(0,0,20)); //Wait 20 seconds for messages with the specified session ID.

            var message = session.Receive();
           
            try
            {
                // Process message from subscription
                Console.WriteLine("\n**Message Received**");
                Console.WriteLine("Body: " + message.GetBody<string>());
                Console.WriteLine("SessionID: " + message.SessionId + " (must only be " + messageId + ")");
                    

                // Remove message from subscription
                message.Complete();
            }
            catch (Exception exc)
            {
                // Indicates a problem, unlock message in queue
                message.Abandon();
                Console.WriteLine("* Error: " + exc.Message);
            }
        }
    }
}
