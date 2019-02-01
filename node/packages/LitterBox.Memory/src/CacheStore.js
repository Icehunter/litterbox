// @flow

import { LitterBoxItem } from 'litterbox';
import { CacheItem } from './CacheItem';
import { MemoryConfiguration } from './MemoryConfiguration';

type Cache = {
  [index: string]: CacheItem
};

export class CacheStore {
  constructor(configuration: MemoryConfiguration) {
    this._configuration = configuration;
    this._expirationTimer = setInterval(() => {
      Object.entries(this._cache).forEach(([k, v]) => {
        if (v.IsExpired) {
          Reflect.deleteProperty(this._cache, k);
        }
      });
    }, configuration.ExpirationScanFrequency);
  }
  _cache: Cache = {};
  _configuration: MemoryConfiguration;
  _expirationTimer: number;
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
