import { ConnectionPool, LitterBox } from '@icehunter/litterbox';
import { RedisConfiguration } from './RedisConfiguration';
import { RedisConnection } from './RedisConnection';

export class RedisBox {
  constructor(configuration: RedisConfiguration) {
    this._configuration = configuration;
  }
  static GetInstance = async (configuration: RedisConfiguration): Promise<LitterBox> => {
    const instance = new LitterBox({
      type: 'LitterBox.Redis',
      pool: new ConnectionPool(configuration),
      configuration,
      connection: new RedisConnection(configuration)
    });
    return await instance.Initialize();
  };
  _configuration: RedisConfiguration;
}
