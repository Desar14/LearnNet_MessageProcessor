using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LearnNet_MessageProcessor
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json");

            var config = configuration.Build();
            
            ServiceBusClient client = new ServiceBusClient(
                    config.GetConnectionString("ServiceBus:Namespace"),
                    new DefaultAzureCredential());

            // create a processor that we can use to process the messages

            ServiceBusProcessor processor = client.CreateProcessor(config.GetConnectionString("ServiceBus:Topic"), config.GetConnectionString("ServiceBus:Subscription"), new ServiceBusProcessorOptions());

            try
            {
                // add handler to process messages
                processor.ProcessMessageAsync += MessageHandler;

                // add handler to process any errors
                processor.ProcessErrorAsync += ErrorHandler;

                // start processing 
                await processor.StartProcessingAsync();

                Console.WriteLine("Press any key to end the processing");
                Console.ReadKey();

                // stop processing 
                Console.WriteLine("\nStopping the receiver...");
                await processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
            finally
            {
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }

            async Task MessageHandler(ProcessMessageEventArgs args)
            {
                Console.WriteLine("Starting message processing");
                string body = args.Message.Body.ToString();

                var model = JsonSerializer.Deserialize<ProductMessageModel>(body);

                if (model == null)
                {
                    Console.WriteLine("Can't be deserialized");
                    await args.DeadLetterMessageAsync(args.Message, "Can't be deserialized");
                    return;
                }

                var updateModel = new ProductUpdateModel
                {
                    Id = model.Id,
                    Name = model.Name,
                    Price = model.Price,
                    Image = new ItemImageModel
                    {
                        AltText = model.Name,
                        Url = model.ImageUrl?.ToString()
                    }
                };

                using HttpClient client = new HttpClient();

                var serializedModel = JsonSerializer.Serialize(updateModel);

                if (serializedModel == null)
                {
                    Console.WriteLine("Can't be serialized");
                    await args.DeadLetterMessageAsync(args.Message, "Can't be serialized");
                    return;
                }

                var content = new StringContent(serializedModel, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(config.GetConnectionString("CartingService:BaseUrl"), content);

                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Success");
                    // complete the message. messages is deleted from the subscription. 
                    await args.CompleteMessageAsync(args.Message);
                }
                else
                {
                    Console.WriteLine("Can't be processed");
                    await args.DeadLetterMessageAsync(args.Message, "Can't be processed", responseString);
                }
            }

            // handle any errors when receiving messages
            Task ErrorHandler(ProcessErrorEventArgs args)
            {
                Console.WriteLine(args.Exception.ToString());
                return Task.CompletedTask;
            }
        }


    }
}
