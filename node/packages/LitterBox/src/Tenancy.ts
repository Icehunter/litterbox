import { ActionResult, LitterBoxItem } from './Models';
import { ILitterBox, ITenancy } from './Interfaces';

import { TenancyEvents } from './events';
import events from 'events';

export class Tenancy implements ITenancy {
  constructor(caches: ILitterBox[]) {
    this._caches = caches;
    this._emitter = new events.EventEmitter();
  }
  private _caches: ILitterBox[] = [];
  private _emitter: events.EventEmitter;
  emitter = (): events.EventEmitter => this._emitter;
  reconnect = async (): Promise<ActionResult[]> => {
    const results: ActionResult[] = [];
    for (let index = 0; index < this._caches.length; index++) {
      const cache = this._caches[index];
      const cacheType = cache.getType();
      const result = new ActionResult({
        cacheType
      });
      try {
        result.isSuccessful = await cache.reconnect();
      } catch (err) {
        this._emitter.emit(TenancyEvents.errors.RECONNECT, {
          type: cacheType,
          err
        });
        result.error = err;
      }
    }
    return results;
  };
  flush = async (): Promise<ActionResult[]> => {
    const results: ActionResult[] = [];
    for (let index = 0; index < this._caches.length; index++) {
      const cache = this._caches[index];
      const cacheType = cache.getType();
      const result = new ActionResult({
        cacheType
      });
      try {
        result.isSuccessful = await cache.flush();
      } catch (err) {
        this._emitter.emit(TenancyEvents.errors.FLUSH, {
          type: cacheType,
          err
        });
        result.error = err;
      }
    }
    return results;
  };
  getItem = async <T>(key: string): Promise<LitterBoxItem<T> | undefined> => {
    let result: LitterBoxItem<T> | undefined;
    for (let index = 0; index < this._caches.length; index++) {
      result = await this._caches[index].getItem<T>(key);
      if (result) {
        break;
      }
    }
    return result;
  };
  getItemUsingGenerator = async <T>(
    key: string,
    generator: () => Promise<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<LitterBoxItem<T> | undefined> => {
    let result: LitterBoxItem<T> | undefined;
    let foundIndex = 0;
    for (let index = 0; index < this._caches.length; index++) {
      const cache = this._caches[index];
      result = await cache.getItem<T>(key);
      if (result !== undefined) {
        foundIndex = index;
        // if the result is stale; refresh this cache
        if (result.isStale()) {
          cache.setItemFireAndForgetUsingGenerator<T>(key, generator, timeToLive, timeToRefresh);
        }
        break;
      }
    }
    if (result === undefined) {
      foundIndex = this._caches.length;
      const value = await generator();
      result = new LitterBoxItem<T>({
        key,
        timeToLive: timeToLive,
        timeToRefresh: timeToRefresh,
        value
      });
    }
    if (result !== undefined) {
      for (let index = 0; index < foundIndex; index++) {
        const cache = this._caches[index];
        // if the result was stale and was also found in a cache
        // refresh all caches from until the cache it was found in
        if (result.isStale() && foundIndex !== index) {
          cache.setItemFireAndForgetUsingGenerator<T>(key, generator, timeToLive, timeToRefresh);
        } else {
          // else we can just save the result into the cache and not regenerate
          cache.setItemFireAndForget<T>(key, result, timeToLive, timeToRefresh);
        }
      }
    }
    return result;
  };
  getItems = async <T>(keys: string[]): Promise<(LitterBoxItem<T> | undefined)[]> => {
    const results: (LitterBoxItem<T> | undefined)[] = [];
    for (let index = 0; index < keys.length; index++) {
      const result = await this.getItem<T>(keys[index]);
      results.push(result);
    }
    return results;
  };
  getItemsUsingGenerator = async <T>(
    keys: string[],
    generators: (() => Promise<T>)[],
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<(LitterBoxItem<T> | undefined)[]> => {
    const results: (LitterBoxItem<T> | undefined)[] = [];
    for (let index = 0; index < keys.length; index++) {
      const key = keys[index];
      const generator = generators[index];
      const result = await this.getItemUsingGenerator<T>(key, generator, timeToLive, timeToRefresh);
      results.push(result);
    }
    return results;
  };
  setItem = async <T>(
    key: string,
    item: LitterBoxItem<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<ActionResult[]> => {
    const results: ActionResult[] = [];
    for (let index = 0; index < this._caches.length; index++) {
      const cache = this._caches[index];
      const cacheType = cache.getType();
      const result = new ActionResult({
        cacheType
      });
      try {
        result.isSuccessful = await cache.setItem<T>(key, item, timeToLive, timeToRefresh);
      } catch (err) {
        result.error = err;
      }
    }
    return results;
  };
  setItems = async <T>(
    keys: string[],
    items: LitterBoxItem<T>[],
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<ActionResult[][]> => {
    const results: ActionResult[][] = [];
    for (let index = 0; index < keys.length; index++) {
      const key = keys[index];
      const item = items[index];
      const result = await this.setItem<T>(key, item, timeToLive, timeToRefresh);
      results.push(result);
    }
    return results;
  };
  setItemFireAndForget = <T>(
    key: string,
    item: LitterBoxItem<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void => {
    for (let index = 0; index < this._caches.length; index++) {
      this._caches[index].setItemFireAndForget<T>(key, item, timeToLive, timeToRefresh);
    }
  };
  setItemFireAndForgetUsingGenerator = <T>(
    key: string,
    generator: () => Promise<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void => {
    for (let index = 0; index < this._caches.length; index++) {
      this._caches[index].setItemFireAndForgetUsingGenerator<T>(key, generator, timeToLive, timeToRefresh);
    }
  };
  removeItem = async (key: string): Promise<ActionResult[]> => {
    const results: ActionResult[] = [];
    for (let index = 0; index < this._caches.length; index++) {
      const cache = this._caches[index];
      const cacheType = cache.getType();
      const result = new ActionResult({
        cacheType
      });
      try {
        result.isSuccessful = await cache.removeItem(key);
      } catch (err) {
        result.error = err;
      }
    }
    return results;
  };
  removeItems = async (keys: string[]): Promise<ActionResult[][]> => {
    const results: ActionResult[][] = [];
    for (let index = 0; index < keys.length; index++) {
      const key = keys[index];
      const result = await this.removeItem(key);
      results.push(result);
    }
    return results;
  };
}
