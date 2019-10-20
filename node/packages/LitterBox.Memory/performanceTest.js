const { Tenancy } = require('@icehunter/litterbox');
const { MemoryBox, MemoryConfiguration } = require('./lib');

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
  try {
    const memory = await MemoryBox.GetInstance(new MemoryConfiguration());
    const caching = new Tenancy([memory]);

    console.log('setting item: ', { testing: 123 });

    await caching.SetItem('s', { testing: 123 });

    const start = Date.now();

    console.log('getting item: ', await caching.GetItem('s'));

    for (let i = 0; i < 1000000; i++) {
      await caching.GetItem('s');
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
