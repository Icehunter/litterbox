import { IConnection, LitterBoxItem } from '@icehunter/litterbox';
import { promisifyAll } from 'bluebird';
import { createClient, RedisClient } from 'redis';
import { RedisConfiguration } from './RedisConfiguration';

export class RedisConnection implements IConnection {
  constructor(configuration: RedisConfiguration) {
    this._configuration = configuration;
    const { host, port, password, dataBaseID } = this._configuration;
    const options = {
      host,
      port,
      password,
      db: dataBaseID,
      // variable is option and must be snake_case
      // eslint-disable-next-line @typescript-eslint/camelcase
      return_buffers: this._configuration.useGZIPCompression
    };
    if (!options.password) {
      Reflect.deleteProperty(options, 'password');
    }
    this._cache = promisifyAll(createClient(options));
  }
  private _cache: RedisClient;
  private _configuration: RedisConfiguration;
  getItem = async (key: string): Promise<LitterBoxItem | null> => {
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
          if (this._configuration.useGZIPCompression) {
            litter = LitterBoxItem.fromBuffer(Buffer.from(item, 'utf8'));
          } else {
            litter = LitterBoxItem.fromJSONString(item.toString());
          }
          resolve(litter);
        });
      } catch (err) {
        reject(err);
      }
    });
  };
  setItem = async (key: string, item: LitterBoxItem, timeToLive?: number, timeToRefresh?: number): Promise<boolean> => {
    return new Promise((resolve, reject) => {
      try {
        const litter = item.clone();
        litter.timeToLive = timeToLive || item.timeToLive;
        litter.timeToRefresh = timeToRefresh || item.timeToRefresh;
        const cacheItem = this._configuration.useGZIPCompression ? litter.toBuffer() : litter.toJSONString();
        // we ignore the next line as the types for redis don't allow buffer BUT redis itself does
        // eslint-disable-next-line @typescript-eslint/ban-ts-ignore
        // @ts-ignore
        this._cache.HSET(key, 'litter', cacheItem, (err) => {
          if (err) {
            return reject(err);
          }
          if (litter.timeToLive) {
            this._cache.EXPIRE(key, litter.timeToLive);
          }
          resolve(true);
        });
      } catch (err) {
        reject(err);
      }
    });
  };
  removeItem = async (key: string): Promise<boolean> => {
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
  };
  connect = async (): Promise<void> => {};
  reconnect = async (): Promise<void> => {
    if (this._cache) {
      this._cache.quit();
    }
    await this.connect();
  };
  flush = async (): Promise<boolean> => {
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
