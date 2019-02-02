// @flow

import { LitterBoxItem } from '@icehunter/litterbox';
import { CacheItem } from './CacheItem';
import { MemoryConfiguration } from './MemoryConfiguration';

type Cache = {
  [key: string]: CacheItem
};

export class CacheStore {
  constructor(configuration: MemoryConfiguration) {
    this._configuration = configuration;
    this._expirationTimer = setInterval(() => {
      Object.keys(this._cache).forEach((key) => {
        const item = this._cache[key];
        if (item.IsExpired()) {
          Reflect.deleteProperty(this._cache, key);
        }
      });
    }, configuration.ExpirationScanFrequency);
  }
  _cache: Cache = {};
  _configuration: MemoryConfiguration;
  _expirationTimer: IntervalID;
  Flush = () => {
    this._cache = {};
  };
  Get = (key: string): LitterBoxItem => {
    const item = this._cache[key];
    if (item) {
      return item;
    }
    return null;
  };
  Set = (key: string, value: any, expiry: ?number = null) => {
    var item = new CacheItem({
      Expiry: this._configuration.DefaultTimeToLive,
      Value: value
    });
    if (expiry) {
      item.Expiry = expiry;
    }
    this._cache[key] = value;
  };
  Delete = (key: string): boolean => {
    Reflect.deleteProperty(this._cache, key);
    return true;
  };
}
