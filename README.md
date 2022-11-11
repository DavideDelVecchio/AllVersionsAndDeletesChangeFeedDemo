# All Versions and Deletes change feed mode demo

This demo shows how to read the change feed in All Versions and Deletes mode using the [pull model](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/change-feed-pull-model?tabs=dotnet). This feature is in private preview. To sign up, fill out the form at [https://aka.ms/changefeed-preview](https://aka.ms/changefeed-preview).

## Setup

This demo will create a new container for you if one doesn't exist. Enter the database and container names along with the account connection string in the `appsettings.json` file.

1. Rename `appsettings.sample.json` to `appsettings.json`
2. Enter your Azure Cosmos DB for NoSQL account connection string as the `ConnectionString` value
3. Optionally, change the name of the `DatabaseName` and `ContainerName`

> Note: This demo expects the partition key for the container to be `/BuyerState`. If you choose to use an existing container with a different partition key, be sure to update the application to use the correct partition key property.

## Run the application

Enter `CTRL + F5` in Visual Studio or enter `dotnet run` from the command line in the `AllVersionsAndDeletesChangeFeedDemo` directory.
