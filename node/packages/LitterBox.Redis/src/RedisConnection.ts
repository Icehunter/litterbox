import { IConnection, LitterBoxItem } from '@icehunter/litterbox';
import { RedisClientType, createClient } from 'redis';

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
      return_buffers: this._configuration.useGZIPCompression
    };
    if (!options.password) {
      Reflect.deleteProperty(options, 'password');
    }

    this._cache = createClient(options);
  }
  private _cache: RedisClientType;
  private _configuration: RedisConfiguration;
  getItem = async <T>(key: string): Promise<LitterBoxItem<T> | undefined> => {
    const item = await this._cache.HGET(key, 'litter');
    if (!item) {
      return;
    }
    let litter: LitterBoxItem<T> | undefined;
    if (this._configuration.useGZIPCompression) {
      litter = LitterBoxItem.fromBuffer(Buffer.from(item, 'utf8'));
    } else {
      litter = LitterBoxItem.fromJSONString(item.toString());
    }
    return litter;
  };
  setItem = async <T>(
    key: string,
    item: LitterBoxItem<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<boolean> => {
    const litter = {
      ...item.clone(),
      timeToLive: timeToLive ?? item.timeToLive ?? this._configuration.defaultTimeToLive,
      timeToRefresh: timeToRefresh ?? item.timeToRefresh ?? this._configuration.defaultTimeToRefresh
    };
    const cacheItem = this._configuration.useGZIPCompression ? litter.toBuffer() : litter.toJSONString();
    await this._cache.HSET(key, 'litter', cacheItem);
    await this._cache.EXPIRE(key, litter.timeToLive);
    return true;
  };
  removeItem = async (key: string): Promise<boolean> => {
    await this._cache.DEL(key);
    return true;
  };
  // eslint-disable-next-line @typescript-eslint/no-empty-function
  connect = async (): Promise<void> => {
    await this._cache.connect();
  };
  reconnect = async (): Promise<void> => {
    if (this._cache) {
      this._cache.quit();
    }
    await this.connect();
  };
  flush = async (): Promise<boolean> => {
    await this._cache.FLUSHALL();
    return true;
  };
}
