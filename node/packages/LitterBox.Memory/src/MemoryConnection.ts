import { IConnection, LitterBoxItem } from '@icehunter/litterbox';

import { ICache } from './ICache';
import { MemoryConfiguration } from './MemoryConfiguration';

export class MemoryConnection implements IConnection {
  constructor(configuration: MemoryConfiguration) {
    this._configuration = configuration;

    const scanner = (): void => {
      clearInterval(this._expirationTimer);
      Promise.resolve(async () => {
        const keys = Object.keys(this._cache);
        for (const key of keys) {
          const item = await this.getItem(key);
          if (item) {
            if (item.isExpired()) {
              await this.removeItem(key);
            }
          }
        }
      }).then(() => {
        this._expirationTimer = setInterval(scanner, configuration.expirationScanFrequency);
      });
    };

    this._expirationTimer = setInterval(scanner, configuration.expirationScanFrequency);
  }
  private _cache: ICache = {};
  private _configuration: MemoryConfiguration;
  private _expirationTimer: NodeJS.Timer;
  getItem = async <T>(key: string): Promise<LitterBoxItem<T> | undefined> => {
    const item = this._cache[key];
    if (item) {
      let litter: LitterBoxItem<T> | undefined;
      if (this._configuration.useGZIPCompression) {
        if (item instanceof Buffer) {
          litter = LitterBoxItem.fromBuffer<T>(item);
        }
      } else {
        litter = item as LitterBoxItem<T>;
      }
      if (litter) {
        if (litter.isExpired()) {
          await this.removeItem(key);
          return;
        }
        return litter;
      }
    }
    return;
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
    const cacheItem = this._configuration.useGZIPCompression ? litter.toBuffer() : litter;
    this._cache[key] = cacheItem;
    return true;
  };
  removeItem = async (key: string): Promise<boolean> => {
    Reflect.deleteProperty(this._cache, key);
    return true;
  };
  connect = async (): Promise<void> => {
    this._cache = {};
  };
  reconnect = async (): Promise<void> => {
    this._cache = {};
  };
  flush = async (): Promise<boolean> => {
    this._cache = {};
    return true;
  };
}
