# `litterbox`

LitterBox is an abstract caching system which supports a multi-layer caching setup; with backfill and high-performance in mind.

The idea behind this library is that you can provide it a list of cache implementations and when calling commands on the main interface you will in turn have those acted upon within the cache layers in priority.

# `caches`

- [litterbox-memory](https://github.com/Icehunter/litterbox/tree/master/node/packages/LitterBox.Memory)
- [litterbox-redis](https://github.com/Icehunter/litterbox/tree/master/node/packages/LitterBox.Redis)

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

  const memoryBox = await MemoryBox.GetInstance(new MemoryConfiguration({
    DefaultTimeToLive: 1 * 60 * 60 * 1000,
    DefaultTimeToRefresh: 5 * 1000,
    PoolSize: 1,
    UseGZIPCompression: false,
    ExpirationScanFrequency: 30 * 1000
  }));

  const redisBox = await RedisBox.GetInstance(new RedisConfiguration({
    DefaultTimeToLive: 1 * 60 * 60 * 1000,
    DefaultTimeToRefresh: 5 * 1000,
    PoolSize: 5,
    UseGZIPCompression: false,
    DataBaseID: 0,
    Host: '127.0.0.1',
    Password: '',
    Port: 6379
  }));

  const tenancy = new Tenancy([memory, redis]);
};

init().then(() => process.exit(0));
```
