# `litterbox`

LitterBox is an abstract caching system which supports a multi-layer caching setup; with backfill and high-performance in mind.

The idea behind this library is that you can provide it a list of cache implementations and when calling commands on the main interface you will in turn have those acted upon within the cache layers in priority.

# `caches`

- [litterbox-memory](https://github.com/Icehunter/litterbox/tree/master/node/packages/LitterBox.Memory)
- [litterbox-redis](https://github.com/Icehunter/litterbox/tree/master/node/packages/LitterBox.Redis)

# `cool features`

I'd like to think these are some cool features

- Probably... lot's of builtin error handling
- When something isn't found in a cache it will continue to search through the other levels until it finds it. Once it finds this item it will return it to the user but send a backfill (fire and forget) request to refill the caches that are missing the data
- It's pretty easy to write another cache layer

# `installation`

```shell
yarn add @icehunter/litterbox-<cachename>
```

# `initialization`

The following code is a full sample (all options) initialization of the **Tenancy**.

```javascript
// all usage of this library assumes it is function as a promise chain or uses async/await
// all values are the default set by the libraries

require('@babel/register');

const { LitterBoxItem, Tenancy } = require('litterbox');
const { MemoryBox, MemoryConfiguration } = require('litterbox-memory');
const { RedisBox, RedisConfiguration } = require('litterbox-redis');

const init = async () => {
  // get a memorybox instance
  const memoryBox = await MemoryBox.GetInstance(
    new MemoryConfiguration({
      // how many milliseconds until it's deleted
      defaultTimeToLive: 1 * 60 * 60 * 1000,
      // how many milliseconds until it's stale
      defaultTimeToRefresh: 5 * 1000,
      // how many connections to pool up
      poolSize: 1,
      // use gzip compression/buffers to save space
      // this drops performance in the node app
      useGZIPCompression: false,
      // how often to scan for expired items
      expirationScanFrequency: 30 * 1000
    })
  );

  // get a redisbox instance
  const redisBox = await RedisBox.GetInstance(
    new RedisConfiguration({
      // how many milliseconds until it's deleted
      defaultTimeToLive: 1 * 60 * 60 * 1000,
      // how many milliseconds until it's stale
      defaultTimeToRefresh: 5 * 1000,
      // how many connections to pool up
      poolSize: 1,
      // use gzip compression/buffers to save space
      // this drops performance in the node app
      useGZIPCompression: false,
      // default database to store data
      dataBaseID: 0,
      // host of redis server
      host: '127.0.0.1',
      // password if using authentication
      Password: '',
      // port of redis server
      port: 6379
    })
  );

  // create a tenancy of caches
  // the order matters as the first is your primary cache
  const tenancy = new Tenancy([memory, redis]);

  // there's a few different methods

  // get item(s) from cache by key
  const item = await tenancy.GetItem('a');
  const items = await tenancy.GetItems(['a', 'b', 'c']);

  // get item(s) from the cache by key, but if missing generate new data
  // imagine that your generator promise is actually a DB call to go fetch the data, or another type of DB
  const generatedItem = await tenancy.GetItemUsingGenerator(
    'a',
    async () => {
      return { testing: 123 };
    },
    360000,
    30
  );
  const generatedItems = await tenancy.GetItemsUsingGenerator(
    ['a', 'b', 'c'],
    [
      async () => {
        return { testing: 123 };
      },
      async () => {
        return { testing: 456 };
      },
      async () => {
        return { testing: 789 };
      }
    ],
    360000,
    30
  );

  // the previous generators also allow you to pass timeToLive and timeToRefresh as the last two params to GetX. In "seconds"
  // if you don't pass those variables; then it actually uses the default caching times setup in the config of the cache when making the tenancy. These are per query overrides
};

init().then(() => process.exit(0));
```

# `notes`

When using generators you will be responding with normal pure responses.

The response of gets and the item passed to set commands that are not generators however will be of type [LitterBoxItem](https://github.com/Icehunter/litterbox/blob/master/node/packages/LitterBox/src/Models/LitterBoxItem.js)

Additionally the following properties are set by default when making a new item:

```javascript
  constructor({
    CacheType = 'INITIAL',
    Created,
    Key,
    TimeToLive,
    TimeToRefresh,
    Value
  }: Props) {
    this.cacheType = cacheType;
    this.created = new Date(created);
    if (this.created.toString() === 'Invalid Date') {
      this.created = new Date();
    }
    this.key = key;
    this.timeToLive = timeToLive;
    this.timeToRefresh = timeToRefresh;
    this.value = value;
}
```

When the actual item is saved in the appropriate cache and when it comes out (depending on which cache was hit) it will set CacheType and Key if they aren't already set.

# `known thingies`

- There's probably some bugs I don't know about...
