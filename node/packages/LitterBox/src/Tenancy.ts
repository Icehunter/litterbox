import { ActionResult, LitterBoxItem } from './Models';
import { ILitterBox, ITenancy } from './Interfaces';

export class Tenancy implements ITenancy {
  constructor(caches: ILitterBox[]) {
    this._caches = caches;
  }
  private _caches: ILitterBox[] = [];
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
        result.error = err;
      }
    }
    return results;
  };
  getItem = async <T>(key: string): Promise<LitterBoxItem<T> | null> => {
    let result: LitterBoxItem<T> | null = null;
    for (let index = 0; index < this._caches.length; index++) {
      result = await this._caches[index].getItem(key);
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
  ): Promise<LitterBoxItem<T> | null> => {
    let result: LitterBoxItem<T> | null = null;
    let foundIndex = 0;
    for (let index = 0; index < this._caches.length; index++) {
      const cache = this._caches[index];
      result = await cache.getItem(key);
      if (result != null) {
        foundIndex = index;
        // if the result is stale; refresh this cache only at this time
        if (result.isStale()) {
          cache.setItemFireAndForgetUsingGenerator(key, generator, timeToLive, timeToRefresh);
        }
        break;
      }
    }
    if (result === null) {
      foundIndex = this._caches.length;
      result = new LitterBoxItem<T>({
        key,
        timeToLive: timeToLive,
        timeToRefresh: timeToRefresh,
        value: await generator()
      });
    }
    if (result !== null) {
      for (let index = 0; index < foundIndex; index++) {
        const cache = this._caches[index];
        // if the result was stale and was also found in a cache
        // refresh all caches from until the cache it was found in
        if (result.isStale() && result.cacheType != null) {
          cache.setItemFireAndForgetUsingGenerator(key, generator, timeToLive, timeToRefresh);
        } else {
          // else we can just save the result into the cache and not regenerate
          cache.setItemFireAndForget(key, result, timeToLive, timeToRefresh);
        }
      }
    }
    return result;
  };
  getItems = async <T>(keys: string[]): Promise<(LitterBoxItem<T> | null)[]> => {
    const results: (LitterBoxItem<T> | null)[] = [];
    for (let index = 0; index < keys.length; index++) {
      const result = await this.getItem(keys[index]);
      results.push(result);
    }
    return results;
  };
  getItemsUsingGenerator = async <T>(
    keys: string[],
    generators: (() => Promise<T>)[],
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<(LitterBoxItem<T> | null)[]> => {
    const results: (LitterBoxItem<T> | null)[] = [];
    for (let index = 0; index < keys.length; index++) {
      const key = keys[index];
      const generator = generators[index];
      const result = await this.getItemUsingGenerator(key, generator, timeToLive, timeToRefresh);
      results.push(result);
    }
    return results;
  };
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
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
        result.isSuccessful = await cache.setItem(key, item, timeToLive, timeToRefresh);
      } catch (err) {
        result.error = err;
      }
    }
    return results;
  };
  setItems = async <T>(
    keys: string[],
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    items: LitterBoxItem<T>[],
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<ActionResult[][]> => {
    const results: ActionResult[][] = [];
    for (let index = 0; index < keys.length; index++) {
      const key = keys[index];
      const item = items[index];
      try {
        const result = await this.setItem(key, item, timeToLive, timeToRefresh);
        results.push(result);
      } catch (err) {
        const result = new ActionResult({
          cacheType: err.message,
          error: err
        });
        results.push([result]);
      }
    }
    return results;
  };
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  setItemFireAndForget = <T>(
    key: string,
    item: LitterBoxItem<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void => {
    for (let index = 0; index < this._caches.length; index++) {
      this._caches[index].setItemFireAndForget(key, item, timeToLive, timeToRefresh);
    }
  };
  setItemFireAndForgetUsingGenerator = <T>(
    key: string,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    generator: () => Promise<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void => {
    for (let index = 0; index < this._caches.length; index++) {
      this._caches[index].setItemFireAndForgetUsingGenerator(key, generator, timeToLive, timeToRefresh);
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
      try {
        const result = await this.removeItem(key);
        results.push(result);
      } catch (err) {
        const result = new ActionResult({
          cacheType: err.message,
          error: err
        });
        results.push([result]);
      }
    }
    return results;
  };
}
