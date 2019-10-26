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
  getItem = async (key: string): Promise<LitterBoxItem | null> => {
    const item = this._cache[key];
    if (item) {
      let litter;
      if (this._configuration.useGZIPCompression) {
        if (item instanceof Buffer) {
          litter = LitterBoxItem.fromBuffer(item);
        }
      } else {
        if (item instanceof LitterBoxItem) {
          litter = item;
        }
      }
      if (litter) {
        if (litter.isExpired()) {
          await this.removeItem(key);
          return null;
        }
        return litter;
      }
    }
    return null;
  };
  setItem = async (key: string, item: LitterBoxItem, timeToLive?: number, timeToRefresh?: number): Promise<boolean> => {
    const litter = item.clone();
    litter.timeToLive = timeToLive || item.timeToLive;
    litter.timeToRefresh = timeToRefresh || item.timeToRefresh;
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
