import { ILitterBox, ITenancy } from './Interfaces';
import { ActionResult, LitterBoxItem } from './Models';

export class Tenancy implements ITenancy {
  constructor(caches: ILitterBox[]) {
    this._caches = caches;
  }
  _caches: ILitterBox[] = [];
  Reconnect = async (): Promise<ActionResult[]> => {
    const results: ActionResult[] = [];
    for (let index = 0; index < this._caches.length; index++) {
      const cache = this._caches[index];
      const CacheType = cache.GetType();
      const result = new ActionResult({
        CacheType
      });
      try {
        result.IsSuccessful = await cache.Reconnect();
      } catch (err) {
        result.Error = err;
      }
    }
    return results;
  };
  Flush = async (): Promise<ActionResult[]> => {
    const results: ActionResult[] = [];
    for (let index = 0; index < this._caches.length; index++) {
      const cache = this._caches[index];
      const CacheType = cache.GetType();
      const result = new ActionResult({
        CacheType
      });
      try {
        result.IsSuccessful = await cache.Flush();
      } catch (err) {
        result.Error = err;
      }
    }
    return results;
  };
  GetItem = async (key: string): Promise<LitterBoxItem | null> => {
    if (!key) {
      throw new Error(`ArgumentException: (null | undefined) => key`);
    }
    let result: LitterBoxItem | null = null;
    for (let index = 0; index < this._caches.length; index++) {
      result = await this._caches[index].GetItem(key);
      if (result) {
        break;
      }
    }
    return result;
  };
  GetItemUsingGenerator = async (
    key: string,
    generator: () => Promise<any>,
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<LitterBoxItem | null> => {
    if (!key) {
      throw new Error(`ArgumentException: (null | undefined) => key`);
    }
    if (!generator) {
      throw new Error(`ArgumentException: (null | undefined) => generator`);
    }
    let result: LitterBoxItem | null = null;
    let foundIndex = 0;
    for (let index = 0; index < this._caches.length; index++) {
      const cache = this._caches[index];
      result = await cache.GetItem(key);
      if (result != null) {
        foundIndex = index;
        // if the result is stale; refresh this cache only at this time
        if (result.IsStale()) {
          cache.SetItemFireAndForgetUsingGenerator(key, generator, timeToLive, timeToRefresh);
        }
        break;
      }
    }
    if (result === null) {
      foundIndex = this._caches.length;
      result = new LitterBoxItem({
        Key: key,
        TimeToLive: timeToLive,
        TimeToRefresh: timeToRefresh,
        Value: await generator()
      });
    }
    if (result !== null) {
      for (let index = 0; index < foundIndex; index++) {
        const cache = this._caches[index];
        // if the result was stale and was also found in a cache
        // refresh all caches from until the cache it was found in
        if (result.IsStale() && result.CacheType != null) {
          cache.SetItemFireAndForgetUsingGenerator(key, generator, timeToLive, timeToRefresh);
        } else {
          // else we can just save the result into the cache and not regenerate
          cache.SetItemFireAndForget(key, result, timeToLive, timeToRefresh);
        }
      }
    }
    return result;
  };
  GetItems = async (keys: string[]): Promise<(LitterBoxItem | null)[]> => {
    if (!keys) {
      throw new Error(`ArgumentException: (null | undefined) => keys`);
    }
    if (!keys.length) {
      throw new Error(`ArgumentException: (Length < 1) => keys.length`);
    }
    const results: (LitterBoxItem | null)[] = [];
    for (let index = 0; index < keys.length; index++) {
      const result = await this.GetItem(keys[index]);
      results.push(result);
    }
    return results;
  };
  GetItemsUsingGenerator = async (
    keys: string[],
    generators: (() => Promise<any>)[],
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<(LitterBoxItem | null)[]> => {
    if (!keys) {
      throw new Error(`ArgumentException: (null | undefined) => keys`);
    }
    if (!generators) {
      throw new Error(`ArgumentException: (null | undefined) => generators`);
    }
    if (!keys.length) {
      throw new Error(`ArgumentException: (Length < 1) => keys.length`);
    }
    if (!generators.length) {
      throw new Error(`ArgumentException: (Length < 1) => generators.length`);
    }
    if (keys.length !== generators.length) {
      throw new Error(`ArgumentException: (Length < 1) => eys.length !== generators.length`);
    }
    const results: (LitterBoxItem | null)[] = [];
    for (let index = 0; index < keys.length; index++) {
      const key = keys[index];
      const generator = generators[index];
      const result = await this.GetItemUsingGenerator(key, generator, timeToLive, timeToRefresh);
      results.push(result);
    }
    return results;
  };
  SetItem = async (key: string, item: any, timeToLive?: number, timeToRefresh?: number): Promise<ActionResult[]> => {
    if (!key) {
      throw new Error(`ArgumentException: (null | undefined) => key`);
    }
    if (!item) {
      throw new Error(`ArgumentException: (null | undefined) => item`);
    }
    const results: ActionResult[] = [];
    for (let index = 0; index < this._caches.length; index++) {
      const cache = this._caches[index];
      const CacheType = cache.GetType();
      const result = new ActionResult({
        CacheType
      });
      try {
        result.IsSuccessful = await cache.SetItem(key, item, timeToLive, timeToRefresh);
      } catch (err) {
        result.Error = err;
      }
    }
    return results;
  };
  SetItems = async (
    keys: string[],
    items: any[],
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<ActionResult[][]> => {
    if (!keys) {
      throw new Error(`ArgumentException: (null | undefined) => keys`);
    }
    if (!items) {
      throw new Error(`ArgumentException: (null | undefined) => items`);
    }
    if (!keys.length) {
      throw new Error(`ArgumentException: (Length < 1) => keys.length`);
    }
    if (!items.length) {
      throw new Error(`ArgumentException: (Length < 1) => items.length`);
    }
    if (keys.length !== items.length) {
      throw new Error(`ArgumentException: (Length < 1) => eys.length !== items.length`);
    }
    const results: ActionResult[][] = [];
    for (let index = 0; index < keys.length; index++) {
      const key = keys[index];
      const item = items[index];
      try {
        const result = await this.SetItem(key, item, timeToLive, timeToRefresh);
        results.push(result);
      } catch (err) {
        const result = new ActionResult({
          CacheType: err.message,
          Error: err
        });
        results.push([result]);
      }
    }
    return results;
  };
  SetItemFireAndForget = (key: string, item: any, timeToLive?: number, timeToRefresh?: number) => {
    if (!key) {
      throw new Error(`ArgumentException: (null | undefined) => key`);
    }
    if (!item) {
      throw new Error(`ArgumentException: (null | undefined) => item`);
    }
    for (let index = 0; index < this._caches.length; index++) {
      this._caches[index].SetItemFireAndForget(key, item, timeToLive, timeToRefresh);
    }
  };
  SetItemFireAndForgetUsingGenerator = (
    key: string,
    generator: () => Promise<any>,
    timeToLive?: number,
    timeToRefresh?: number
  ) => {
    if (!key) {
      throw new Error(`ArgumentException: (null | undefined) => key`);
    }
    if (!generator) {
      throw new Error(`ArgumentException: (null | undefined) => generator`);
    }
    for (let index = 0; index < this._caches.length; index++) {
      this._caches[index].SetItemFireAndForgetUsingGenerator(key, generator, timeToLive, timeToRefresh);
    }
  };
  RemoveItem = async (key: string): Promise<ActionResult[]> => {
    if (!key) {
      throw new Error(`ArgumentException: (null | undefined) => key`);
    }
    const results: ActionResult[] = [];
    for (let index = 0; index < this._caches.length; index++) {
      const cache = this._caches[index];
      const CacheType = cache.GetType();
      const result = new ActionResult({
        CacheType
      });
      try {
        result.IsSuccessful = await cache.RemoveItem(key);
      } catch (err) {
        result.Error = err;
      }
    }
    return results;
  };
  RemoveItems = async (keys: string[]): Promise<ActionResult[][]> => {
    if (!keys) {
      throw new Error(`ArgumentException: (null | undefined) => keys`);
    }
    if (!keys.length) {
      throw new Error(`ArgumentException: (Length < 1) => keys.length`);
    }
    const results: ActionResult[][] = [];
    for (let index = 0; index < keys.length; index++) {
      const key = keys[index];
      try {
        const result = await this.RemoveItem(key);
        results.push(result);
      } catch (err) {
        const result = new ActionResult({
          CacheType: err.message,
          Error: err
        });
        results.push([result]);
      }
    }
    return results;
  };
}
