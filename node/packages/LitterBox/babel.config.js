process.env.NODE_ENV = process.env.NODE_ENV || 'development';

const preset = require('babel-preset-react-app');

module.exports = (api) => {
  api.cache(true);
  // use the default create from create-react-app but extend it for local use with our own webpack
  const config = preset(api, {
    flow: true,
    helpers: false,
    absoluteRuntime: false
  });

  // since this is only a node module; target node:current
  config.presets[0][1] = {
    targets: {
      node: 'current'
    }
  };

  return config;
};
