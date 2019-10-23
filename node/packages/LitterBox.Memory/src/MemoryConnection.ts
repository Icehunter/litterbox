import { IConnection, LitterBoxItem } from '@icehunter/litterbox';
import { ICache } from './ICache';
import { MemoryConfiguration } from './MemoryConfiguration';

export class MemoryConnection implements IConnection {
  constructor(configuration: MemoryConfiguration) {
    this._configuration = configuration;
    this._expirationTimer = setInterval(() => {
      Object.keys(this._cache).forEach(async (key) => {
        const item = await this.GetItem(key);
        if (item) {
          if (item.IsExpired()) {
            await this.RemoveItem(key);
          }
        }
      });
    }, configuration.ExpirationScanFrequency);
  }
  _cache: ICache = {};
  _configuration: MemoryConfiguration;
  _expirationTimer: NodeJS.Timer;
  GetItem = async (key: string): Promise<LitterBoxItem | null> => {
    if (!key) {
      throw new Error(`ArgumentException: (null | undefined) => key`);
    }
    const item = this._cache[key];
    if (item) {
      let litter;
      if (this._configuration.UseGZIPCompression) {
        if (item instanceof Buffer) {
          litter = LitterBoxItem.FromBuffer(item);
        }
      } else {
        if (item instanceof LitterBoxItem) {
          litter = item;
        }
      }
      if (litter) {
        if (litter.IsExpired()) {
          await this.RemoveItem(key);
          return null;
        }
        return litter;
      }
    }
    return null;
  };
  SetItem = async (key: string, item: LitterBoxItem, timeToLive?: number, timeToRefresh?: number): Promise<boolean> => {
    if (!key) {
      throw new Error(`ArgumentException: (null | undefined) => key`);
    }
    if (!item) {
      throw new Error(`ArgumentException: (null | undefined) => item`);
    }
    const litter = item.Clone();
    litter.TimeToLive = timeToLive || item.TimeToLive;
    litter.TimeToRefresh = timeToRefresh || item.TimeToRefresh;
    const cacheItem = this._configuration.UseGZIPCompression ? litter.ToBuffer() : litter;
    this._cache[key] = cacheItem;
    return true;
  };
  RemoveItem = async (key: string): Promise<boolean> => {
    if (!key) {
      throw new Error(`ArgumentException: (null | undefined) => key`);
    }
    Reflect.deleteProperty(this._cache, key);
    return true;
  };
  Connect = async (): Promise<void> => {
    this._cache = {};
  };
  Reconnect = async (): Promise<void> => {
    this._cache = {};
  };
  Flush = async (): Promise<boolean> => {
    this._cache = {};
    return true;
  };
}
