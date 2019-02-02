// @flow

import { IConnection } from '@icehunter/litterbox';
import redis from 'redis';
import { RedisConfiguration } from './RedisConfiguration';

import type { RedisClient } from 'redis';

export class RedisConnection implements IConnection {
  constructor(configuration: RedisConfiguration) {
    this._configuration = configuration;
  }
  Cache: RedisClient;
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
    return new Promise((resolve, reject) => {
      this.Cache.flushall((err) => {
        if (err) {
          return reject(err);
        }
        resolve(true);
      });
    });
  };
}
