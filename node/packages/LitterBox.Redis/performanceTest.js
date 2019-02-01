require('@babel/register');

const { LitterBoxItem, Tenancy } = require('litterbox');
const { RedisBox, RedisConfiguration } = require('./src');

const init = async () => {
  const redis = await RedisBox.GetInstance(
    new RedisConfiguration({
      Port: 6379
    })
  );
  const caching = new Tenancy([redis]);

  console.time('1,000,000 GetItem');

  await caching.SetItem('s', new LitterBoxItem({ Value: { testing: 123 } }));

  for (let i = 0; i < 1000000; i++) {
    await caching.GetItem('s');
  }

  console.timeEnd('1,000,000 GetItem');
};

init().then(() => process.exit(0));
