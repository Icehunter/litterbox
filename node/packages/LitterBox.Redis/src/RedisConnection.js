// @flow

import { IConnection } from '@icehunter/litterbox';
import redis from 'redis';
import { RedisConfiguration } from './RedisConfiguration';

export class RedisConnection implements IConnection {
  constructor(configuration: RedisConfiguration) {
    this._configuration = configuration;
  }
  Cache: redis.RedisClient;
  _configuration: RedisConfiguration;
  Connect = async () => {
    const options = {
      host: this._configuration.Host,
      port: this._configuration.Port,
      password: this._configuration.Password,
      db: this._configuration.DataBaseID,
      return_buffers: true
    };
    if (!options.password) {
      Reflect.deleteProperty(options, 'password');
    }
    this.Cache = redis.createClient(options);
  };
  Reconnect = async () => {
    if (this.Cache) {
      this.Cache.quit();
    }
    await this.Connect();
  };
  Flush = async (): Promise<boolean> => {
    this.Cache.flushdb();
    return true;
  };
}
