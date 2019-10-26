const { Tenancy } = require('@icehunter/litterbox');
const { MemoryBox, MemoryConfiguration } = require('./lib');

const MSToTime = (duration) => {
  const milliseconds = (duration % 1000) / 100;
  const seconds = Math.floor((duration / 1000) % 60);
  const minutes = Math.floor((duration / (1000 * 60)) % 60);
  const hours = Math.floor((duration / (1000 * 60 * 60)) % 24);

  const formattedHours = `${hours < 10 ? '0' + hours : hours}h`;
  const formattedMinutes = `${minutes < 10 ? '0' + minutes : minutes}m`;
  const formattedSeconds = `${seconds < 10 ? '0' + seconds : seconds}s`;
  const formattedMilliseconds = `${milliseconds}ms`;

  return [formattedHours, formattedMinutes, formattedSeconds, formattedMilliseconds].join(':');
};

const init = async () => {
  try {
    const memory = await MemoryBox.getInstance(new MemoryConfiguration());
    const caching = new Tenancy([memory]);

    console.log('setting item: ', { testing: 123 });

    await caching.setItem('s', { testing: 123 });

    const start = Date.now();

    console.log('getting item: ', await caching.getItem('s'));

    for (let i = 0; i < 1000000; i++) {
      await caching.getItem('s');
    }

    const end = Date.now();
    const duration = end - start;

    console.log(`1,000,000 GetItem Ops Took: ${MSToTime(duration)}`);
    console.log(`${Math.floor(1000000 / (duration / 1000))} Ops/second`);
  } catch (err) {
    console.log(err);
  }
};

init().then(() => process.exit(0));
