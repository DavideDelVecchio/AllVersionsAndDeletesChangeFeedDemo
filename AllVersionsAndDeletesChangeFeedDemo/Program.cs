using AllVersionsAndDeletesChangeFeedDemo;

ChangeFeedDemo changeFeedDemo = new ChangeFeedDemo();
await changeFeedDemo.GetOrCreateContainer();
await changeFeedDemo.CreateAllVersionsAndDeletesChangeFeedIterator();
await changeFeedDemo.CreateLatestVersionChangeFeedIterator();
await changeFeedDemo.IngestData();
await changeFeedDemo.DeleteData();
await changeFeedDemo.ReadLatestVersionChangeFeed();
await changeFeedDemo.ReadAllVersionsAndDeletesChangeFeed();