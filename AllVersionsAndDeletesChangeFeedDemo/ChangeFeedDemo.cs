using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using AllVersionsAndDeletesChangeFeedDemo.Models;

namespace AllVersionsAndDeletesChangeFeedDemo
{
    internal class ChangeFeedDemo
    {
        private static string connectionString;
        private static CosmosClient cosmosClient;
        public Container container;
        private int deleteItemCounter;
        public string containerName;
        public string databaseName;
        private string? allVersionsContinuationToken;
        private string? latestVersionContinuationToken;

        public ChangeFeedDemo()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                                   .AddJsonFile("AppSettings.json")
                                   .Build();

            connectionString = configuration["ConnectionString"]!;
            databaseName = configuration["DatabaseName"]!;
            containerName = configuration["ContainerName"]!;

            cosmosClient = new CosmosClient(connectionString);

            deleteItemCounter = 0;
        }

        public async Task GetOrCreateContainer()
        {
            Console.WriteLine($"Getting container reference for {containerName}.");

            ContainerProperties properties = new ContainerProperties(containerName, partitionKeyPath: "/BuyerState");

            await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
            container = await cosmosClient.GetDatabase(databaseName).CreateContainerIfNotExistsAsync(properties);
        }

        public async Task CreateAllVersionsAndDeletesChangeFeedIterator()
        {
            Console.WriteLine("Creating ChangeFeedIterator to read the change feed in All Versions and Deletes mode.");

            allVersionsContinuationToken = null;
            FeedIterator<dynamic> allVersionsIterator = container
                .GetChangeFeedIterator<dynamic>(ChangeFeedStartFrom.Now(), ChangeFeedMode.FullFidelity);

            while (allVersionsIterator.HasMoreResults)
            {
                FeedResponse<dynamic> response = await allVersionsIterator.ReadNextAsync();

                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    allVersionsContinuationToken = response.ContinuationToken;
                    break;
                }
            }
        }

        public async Task CreateLatestVersionChangeFeedIterator()
        {
            Console.WriteLine("Creating ChangeFeedIterator to read the change feed in Latest Version mode.");

            latestVersionContinuationToken = null;
            FeedIterator<Item> latestVersionIterator = container
                .GetChangeFeedIterator<Item>(ChangeFeedStartFrom.Now(), ChangeFeedMode.Incremental);

            while (latestVersionIterator.HasMoreResults)
            {
                FeedResponse<Item> response = await latestVersionIterator.ReadNextAsync();

                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    latestVersionContinuationToken = response.ContinuationToken;
                    break;
                }
            }
        }

        public async Task IngestData()
        {
            Console.Clear();

            await Console.Out.WriteLineAsync("Press any key to begin ingesting data.");

            Console.ReadKey(true);

            await Console.Out.WriteLineAsync("Press any key to stop.");

            var tasks = new List<Task>();

            while (!Console.KeyAvailable)
            {
                var item = GenerateItem();
                await container.UpsertItemAsync(item, new PartitionKey(item.BuyerState));
                Console.Write("*");
            }

            await Task.WhenAll(tasks);
        }

        public async Task DeleteData()
        {
            Console.ReadKey(true);
            Console.Clear();
            await Console.Out.WriteLineAsync("Press any key to begin deleting data.");
            Console.ReadKey(true);

            await Console.Out.WriteLineAsync("Press any key to stop");

            while (!Console.KeyAvailable)
            {
                deleteItemCounter++;
                try
                {
                    await container.DeleteItemAsync<Item>(
                       partitionKey: new PartitionKey("WA"),
                       id: deleteItemCounter.ToString());
                    Console.Write("-");
                }
                catch (CosmosException cosmosException) when (cosmosException.StatusCode == HttpStatusCode.NotFound)
                {
                    // Deleting by a random id that might not exist in the container will likely throw errors that are safe to ignore for this purpose
                }
            }
        }

        public async Task ReadLatestVersionChangeFeed()
        {
            Console.ReadKey(true);
            Console.Clear();

            await Console.Out.WriteLineAsync("Press any key to begin reading the change feed in Latest Version mode.");

            Console.ReadKey(true);

            FeedIterator<Item> latestVersionIterator = container.GetChangeFeedIterator<Item>(ChangeFeedStartFrom.ContinuationToken(latestVersionContinuationToken), ChangeFeedMode.Incremental, new ChangeFeedRequestOptions { PageSizeHint = 10 });

            await Console.Out.WriteLineAsync("Press any key to stop.");

            while (latestVersionIterator.HasMoreResults)
            {
                FeedResponse<Item> response = await latestVersionIterator.ReadNextAsync();

                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    latestVersionContinuationToken = response.ContinuationToken;
                    Console.WriteLine($"No new changes");
                } 
                else
                {
                    foreach (Item item in response)
                    {
                        // for any operation
                        Console.WriteLine($"Change in item: {item.Id}. New price: {item.Price}.");
                    }
                }

                if (Console.KeyAvailable)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
        }

        public async Task ReadAllVersionsAndDeletesChangeFeed()
        {
            Console.ReadKey(true);
            Console.Clear();

            await Console.Out.WriteLineAsync("Press any key to start reading the full fidelity change feed.");

            Console.ReadKey(true);

            FeedIterator<dynamic> allVersionsIterator = container.GetChangeFeedIterator<dynamic>(ChangeFeedStartFrom.ContinuationToken(allVersionsContinuationToken), ChangeFeedMode.FullFidelity, new ChangeFeedRequestOptions { PageSizeHint = 10 });

            await Console.Out.WriteLineAsync("Press any key to stop.");

            while (allVersionsIterator.HasMoreResults)
            {

                FeedResponse<dynamic> response = await allVersionsIterator.ReadNextAsync();

                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    allVersionsContinuationToken = response.ContinuationToken;
                    Console.WriteLine($"No new changes");
                }
                else
                {
                    foreach (dynamic r in response)
                    {
                        // if operaiton is delete
                        if (r.metadata.operationType == "delete")
                        {
                            Item item = r.previous.ToObject<Item>();

                            if (r.metadata.timeToLiveExpired == true)
                            {
                                Console.WriteLine($"Operation: {r.metadata.operationType} (due to TTL). Item id: {item.Id}. Previous price: {item.Price}");
                            }
                            else
                            {
                                Console.WriteLine($"Operation: {r.metadata.operationType} (not due to TTL). Item id: {item.Id}. Previous price: {item.Price}");
                            }
                        }
                        //if operation is replace or insert
                        else
                        {
                            Item item = r.current.ToObject<Item>();

                            Console.WriteLine($"Operation: {r.metadata.operationType}. Item id: {item.Id}. Current price: {item.Price}");
                        }
                    }
                } 

                Thread.Sleep(1000);
                if (Console.KeyAvailable)
                {
                    break;
                }
            }
        }

        private static Item GenerateItem()
        {
            Random random = new Random();

            var states = new string[]
            {
                "WA"
            };

            var prices = new double[]
            {
                3.75, 8.00, 12.00, 10.00,
                17.00, 20.00, 14.00, 15.50,
                9.00, 25.00, 27.00, 21.00, 22.50,
                22.50, 32.00, 30.00, 49.99, 35.50,
                55.00, 50.00, 65.00, 31.99, 79.99,
                22.00, 19.99, 19.99, 80.00, 85.00,
                90.00, 33.00, 25.20, 40.00, 87.50, 99.99,
                95.99, 75.00, 70.00, 65.00, 92.00, 95.00,
                72.00, 25.00, 120.00, 105.00, 130.00, 29.99,
                84.99, 12.00, 37.50
            };

            var stateIndex = random.Next(0, states.Length - 1);
            var pricesIndex = random.Next(0, prices.Length - 1);

            return new Item
                {
                    Id = random.Next(1, 999).ToString(),
                    Price = prices[pricesIndex],
                    BuyerState = states[stateIndex]
                };
        }
    }
}
