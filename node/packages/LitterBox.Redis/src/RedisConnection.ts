import { IConnection, LitterBoxItem } from '@icehunter/litterbox';
import { promisifyAll } from 'bluebird';
import { createClient, RedisClient } from 'redis';
import { RedisConfiguration } from './RedisConfiguration';

export class RedisConnection implements IConnection {
  constructor(configuration: RedisConfiguration) {
    this._configuration = configuration;
    const { Host, Port, Password, DataBaseID } = this._configuration;
    const options = {
      host: Host,
      port: Port,
      password: Password,
      db: DataBaseID,
      return_buffers: this._configuration.UseGZIPCompression
    };
    if (!options.password) {
      Reflect.deleteProperty(options, 'password');
    }
    this._cache = promisifyAll(createClient(options));
  }
  _cache: RedisClient;
  _configuration: RedisConfiguration;
  GetItem = async (key: string): Promise<LitterBoxItem | null> => {
    if (!key) {
      throw new Error(`ArgumentException: (null | undefined) => key`);
    }
    return new Promise((resolve, reject) => {
      try {
        this._cache.HGET(key, 'litter', (err, item) => {
          if (err) {
            return reject(err);
          }
          if (!item) {
            return resolve(null);
          }
          let litter = null;
          if (this._configuration.UseGZIPCompression) {
            litter = LitterBoxItem.FromBuffer((item as unknown) as Buffer);
          } else {
            litter = LitterBoxItem.FromJSONString(item.toString());
          }
          resolve(litter);
        });
      } catch (err) {
        reject(err);
      }
    });
  };
  SetItem = async (key: string, item: LitterBoxItem, timeToLive?: number, timeToRefresh?: number): Promise<boolean> => {
    if (!key) {
      throw new Error(`ArgumentException: (null | undefined) => key`);
    }
    if (!item) {
      throw new Error(`ArgumentException: (null | undefined) => item`);
    }
    return new Promise((resolve, reject) => {
      try {
        const litter = item.Clone();
        litter.TimeToLive = timeToLive || item.TimeToLive;
        const cacheItem = this._configuration.UseGZIPCompression ? litter.ToBuffer() : litter.ToJSONString();
        this._cache.HSET(key, 'litter', cacheItem.toString(), (err) => {
          if (err) {
            return reject(err);
          }
          if (litter.TimeToLive) {
            this._cache.EXPIRE(key, litter.TimeToLive);
          }
          resolve(true);
        });
      } catch (err) {
        reject(err);
      }
    });
  };
  RemoveItem = async (key: string): Promise<boolean> => {
    if (!key) {
      throw new Error(`ArgumentException: (null | undefined) => key`);
    }
    return new Promise((resolve, reject) => {
      try {
        this._cache.DEL(key, (err) => {
          if (err) {
            return reject(err);
          }
          resolve(true);
        });
      } catch (err) {
        reject(err);
      }
    });
    return true;
  };
  Connect = async () => {};
  Reconnect = async () => {
    if (this._cache) {
      this._cache.quit();
    }
    await this.Connect();
  };
  Flush = async (): Promise<boolean> => {
    return new Promise((resolve, reject) => {
      try {
        this._cache.FLUSHALL((err) => {
          if (err) {
            return reject(err);
          }
          resolve(true);
        });
      } catch (err) {
        reject(err);
      }
    });
  };
}
