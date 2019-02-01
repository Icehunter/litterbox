require('@babel/register');

const { LitterBoxItem, Tenancy } = require('litterbox');
const { MemoryBox, MemoryConfiguration } = require('./src');

const init = async () => {
  const memory = await MemoryBox.GetInstance(new MemoryConfiguration());
  const caching = new Tenancy([memory]);

  console.time('1,000,000 GetItem');

  await caching.SetItem('s', new LitterBoxItem({ Value: { testing: 123 } }));

  for (let i = 0; i < 1000000; i++) {
    await caching.GetItem('s');
  }

  console.timeEnd('1,000,000 GetItem');
};

init().then(() => process.exit(0));
