// @flow

import { ILitterBox, ITenancy } from './Interfaces';
import { ExceptionEvent, FlushResult, LitterBoxItem, ReconnectionResult, StorageResult } from './Models';

export class Tenancy implements ITenancy {
  constructor(caches: ILitterBox[]) {
    this._caches = caches;

    this._caches.forEach((cache) => {
      // TODO: subscribe to exception event listeners
      // cache.ExceptionEvent += this.CacheOnExceptionEvent;
    });
  }
  _caches: ILitterBox[] = [];
  RaiseException = (err: any) => {
    console.error(err);
    // this.ExceptionEvent?.Invoke(new ExceptionEvent(err));
  };
  CacheOnExceptionEvent = (event: ExceptionEvent) => {
    // this.ExceptionEvent?.Invoke(error);
  };
  // public event EventHandler<ExceptionEvent> ExceptionEvent;
  Reconnect = async (): Promise<(?ReconnectionResult)[]> => {
    const results = [];

    for (let index = 0; index < this._caches.length; index++) {
      const cache = this._caches[index];
      const IsSuccessful = await cache.Reconnect();
      results.push(
        new ReconnectionResult({
          CacheType: cache.GetType(),
          IsSuccessful
        })
      );
    }

    return results;
  };
  Flush = async (): Promise<(?FlushResult)[]> => {
    const results = [];

    for (let index = 0; index < this._caches.length; index++) {
      const cache = this._caches[index];
      const IsSuccessful = await cache.Flush();
      results.push(
        new FlushResult({
          CacheType: cache.GetType(),
          IsSuccessful
        })
      );
    }

    return results;
  };
  GetItem = async (key: string): Promise<?LitterBoxItem> => {
    if (!key) {
      // this.RaiseException(new ArgumentException($"{nameof(this.GetItem)}=>{nameof(key)} Cannot Be NullOrWhiteSpace"));
      return null;
    }

    for (let index = 0; index < this._caches.length; index++) {
      const cache = this._caches[index];
      const result = await cache.GetItem(key);
      if (result != null) {
        result.Key = key;
        result.CacheType = cache.GetType();
        return result;
      }
    }

    return null;
  };
  GetItemGenerated = async (
    key: string,
    generator: () => Promise<any>,
    timeToRefresh: number,
    timeToLive: number
  ): Promise<?LitterBoxItem> => {
    if (!key) {
      // this.RaiseException(new ArgumentException($"{nameof(this.GetItemGenerated)}=>{nameof(key)} Cannot Be NullOrWhiteSpace"));
      return null;
    }
    if (!generator) {
      // this.RaiseException(new ArgumentException($"{nameof(this.GetItemGenerated)}=>{nameof(generator)} Cannot Be Null"));
      return null;
    }

    let foundIndex = 0;
    let result = null;

    for (let i = 0; i < this._caches.length; i++) {
      const cache = this._caches[i];
      result = await cache.GetItem(key);
      if (result != null) {
        foundIndex = i;

        result.Key = key;
        result.CacheType = cache.GetType();

        // if the item is stale; refresh this cache only at this time
        if (result.IsStale()) {
          cache.SetItemFireAndForgetGenerated(key, generator, timeToRefresh, timeToLive);
        }

        break;
      }
    }

    let toLive = null;
    let toRefresh = null;

    if (timeToLive != null) {
      toLive = timeToLive;
    }

    if (timeToRefresh != null) {
      toRefresh = timeToRefresh;
    }

    if (result == null) {
      foundIndex = this._caches.length;
      try {
        result = new LitterBoxItem({
          Value: await generator(),
          TimeToLive: toLive,
          TimeToRefresh: toRefresh
        });
      } catch (err) {
        this.RaiseException(err);
      }
    }

    if (result != null) {
      for (let i = 0; i < foundIndex; i++) {
        const cache = this._caches[i];

        // if the result was stale and was also found in a cache
        // refresh all caches from until the cache it was found in
        if (result.IsStale() && result.CacheType != null) {
          cache.SetItemFireAndForgetGenerated(key, generator, timeToRefresh, timeToLive);
        } else {
          // else we can just save the item into the cache and not regenerate
          cache.SetItemFireAndForget(key, result);
        }
      }
    }

    return result;
  };
  GetItems = async (keys: string[]): Promise<(?LitterBoxItem)[]> => {
    if (!keys) {
      // this.RaiseException(new ArgumentException($"{nameof(this.GetItems)}=>{nameof(key)} Cannot Be NullOrWhiteSpace"));
      return null;
    }
    if (!keys.length) {
      // this.RaiseException(new ArgumentException($"{nameof(this.GetItems)}=>{nameof(keys)}.Length Must Be Greater Than 0"));
      return null;
    }

    const results = [];

    for (let index = 0; index < keys.length; index++) {
      const key = keys[index];
      const result = await this.GetItem(key);
      results.push(result);
    }

    return results;
  };
  GetItemsGenerated = async (
    keys: string[],
    generators: (() => Promise<any>)[],
    timeToRefresh: number,
    timeToLive: number
  ): Promise<Array<LitterBoxItem | null> | null> => {
    if (!keys) {
      // this.RaiseException(new ArgumentException($"{nameof(this.GetItemsGenerated)}=>{nameof(keys)} Cannot Be Null"));
      return null;
    }
    if (!generators) {
      // this.RaiseException(new ArgumentException($"{nameof(this.GetItemsGenerated)}=>{nameof(generators)} Cannot Be Null"));
      return null;
    }
    if (!keys.length) {
      // this.RaiseException(new ArgumentException($"{nameof(this.GetItemsGenerated)}=>{nameof(keys)}.Length Must Be Greater Than 0"));
      return null;
    }
    if (!generators.length) {
      // this.RaiseException(new ArgumentException($"{nameof(this.GetItemsGenerated)}=>{nameof(generators)}.Length Must Be Greater Than 0"));
      return null;
    }
    if (keys.length !== generators.length) {
      // this.RaiseException(new ArgumentException($"{nameof(this.GetItemsGenerated)}=>{nameof(keys)}.Length/{nameof(generators)}.Length Must Be Equal"));
      return null;
    }

    const results = [];

    for (let index = 0; index < keys.length; index++) {
      const key = keys[index];
      const generator = generators[index];

      if (!key && !generator) {
        results.push(new LitterBoxItem());
      } else {
        const result = await this.GetItemGenerated(key, generator, timeToRefresh, timeToLive);
        results.push(result);
      }
    }

    return results;
  };
  SetItem = async (key: string, litter: LitterBoxItem): Promise<(?StorageResult)[]> => {
    if (!key) {
      // this.RaiseException(new ArgumentException($"{nameof(this.SetItem)}=>{nameof(key)} Cannot Be NullOrWhiteSpace"));
      return null;
    }
    if (!litter) {
      // this.RaiseException(new ArgumentException($"{nameof(this.SetItem)}=>{nameof(litter)} Cannot Be Null"));
      return null;
    }

    const results = [];

    for (let index = 0; index < this._caches.length; index++) {
      const cache = this._caches[index];
      const IsSuccessful = await cache.SetItem(key, litter);
      results.push(
        new StorageResult({
          CacheType: cache.GetType(),
          IsSuccessful
        })
      );
    }

    return results;
  };
  SetItems = async (keys: string[], litters: LitterBoxItem[]): Promise<(?StorageResult)[][]> => {
    if (!keys) {
      // this.RaiseException(new ArgumentException($"{nameof(this.SetItems)}=>{nameof(keys)} Cannot Be Null"));
      return null;
    }
    if (!litters) {
      // this.RaiseException(new ArgumentException($"{nameof(this.SetItems)}=>{nameof(litters)} Cannot Be Null"));
      return null;
    }
    if (!keys.length) {
      // this.RaiseException(new ArgumentException($"{nameof(this.SetItems)}=>{nameof(keys)}.Length Must Be Greater Than 0"));
      return null;
    }
    if (!litters.length) {
      // this.RaiseException(new ArgumentException($"{nameof(this.SetItems)}=>{nameof(litters)}.Length Must Be Greater Than 0"));
      return null;
    }
    if (keys.length !== litters.length) {
      // this.RaiseException(new ArgumentException($"{nameof(this.SetItems)}=>{nameof(keys)}.Length/{nameof(litters)}.Length Must Be Equal"));
      return null;
    }

    const results = [];

    for (let index = 0; index < keys.length; index++) {
      const key = keys[index];
      const litter = litters[index];

      if (!key && !litter) {
        results.push([]);
      } else {
        const result = await this.SetItem(key, litter);
        results.push(result);
      }
    }

    return results;
  };
  SetItemFireAndForget = (key: string, litter: LitterBoxItem): void => {
    if (!key) {
      // this.RaiseException(new ArgumentException($"{nameof(this.SetItemFireAndForget)}=>{nameof(key)} Cannot Be NullOrWhiteSpace"));
      return;
    }
    if (!litter) {
      // this.RaiseException(new ArgumentException($"{nameof(this.SetItemFireAndForget)}=>{nameof(litter)} Cannot Be Null"));
      return;
    }

    for (let index = 0; index < this._caches.length; index++) {
      const cache = this._caches[index];
      cache.SetItemFireAndForget(key, litter);
    }
  };
  SetItemFireAndForgetGenerated = (
    key: string,
    generator: () => Promise<any>,
    timeToRefresh: number,
    timeToLive: number
  ): void => {
    if (!key) {
      // this.RaiseException(new ArgumentException($"{nameof(this.SetItemFireAndForgetGenerated)}=>{nameof(key)} Cannot Be NullOrWhiteSpace"));
      return;
    }
    if (!generator) {
      // this.RaiseException(new ArgumentException($"{nameof(this.SetItemFireAndForgetGenerated)}=>{nameof(generator)} Cannot Be Null"));
      return;
    }

    for (let index = 0; index < this._caches.length; index++) {
      const cache = this._caches[index];
      cache.SetItemFireAndForgetGenerated(key, generator, timeToRefresh, timeToLive);
    }
  };
}
