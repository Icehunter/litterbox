// @flow

import { IConnection } from 'litterbox';
import { CacheStore } from './CacheStore';
import { MemoryConfiguration } from './MemoryConfiguration';

export class MemoryConnection implements IConnection {
  constructor(configuration: MemoryConfiguration) {
    this._configuration = configuration;
  }
  Cache: CacheStore;
  _configuration: MemoryConfiguration;
  Connect = async () => {
    this.Cache = new CacheStore(this._configuration);
  };
  Reconnect = async () => {};
  Flush = async (): Promise<boolean> => {
    this.Cache.Flush();
    return true;
  };
}
