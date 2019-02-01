require('@babel/register');

const { LitterBoxItem, Tenancy } = require('litterbox');
const { RedisBox, RedisConfiguration } = require('./src');

const MSToTime = (duration) => {
  var milliseconds = parseInt((duration % 1000) / 100),
    seconds = parseInt((duration / 1000) % 60),
    minutes = parseInt((duration / (1000 * 60)) % 60),
    hours = parseInt((duration / (1000 * 60 * 60)) % 24);

  hours = hours < 10 ? '0' + hours : hours;
  minutes = minutes < 10 ? '0' + minutes : minutes;
  seconds = seconds < 10 ? '0' + seconds : seconds;

  return hours + 'h: ' + minutes + 'm: ' + seconds + 's: ' + milliseconds + 'ms';
};

const init = async () => {
  const redis = await RedisBox.GetInstance(
    new RedisConfiguration({
      Port: 6379
    })
  );
  const caching = new Tenancy([redis]);

  await caching.SetItem('s', new LitterBoxItem({ Value: { testing: 123 } }));

  const start = Date.now();

  for (let i = 0; i < 1000000; i++) {
    await caching.GetItem('s');
  }

  const end = Date.now();
  const duration = end - start;

  console.log(`1,000,000 GetItem Ops Took: ${MSToTime(duration)}`);
  console.log(`${Math.floor(1000000 / (duration / 1000))} Ops/second`);
};

init().then(() => process.exit(0));
