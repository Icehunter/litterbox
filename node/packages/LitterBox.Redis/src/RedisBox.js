// @flow

import { Compression, ConnectionPool, ILitterBox, LitterBoxItem } from 'litterbox';
import { RedisConfiguration } from './RedisConfiguration';
import { RedisConnection } from './RedisConnection';

type InProcess = {
  [index: string]: boolean
};

export class RedisBox implements ILitterBox {
  constructor(configuration: RedisConfiguration) {
    this._configuration = configuration;

    this.Pool = new ConnectionPool({
      PoolSize: this._configuration.PoolSize
    });
  }
  GetType = (): string => 'LitterBox.Redis';
  RaiseException = (err: any) => {
    console.error(err);
    // this.ExceptionEvent?.Invoke(new ExceptionEvent(err));
  };
  Initialize = async () => {
    await this.Pool.Initialize(new RedisConnection(this._configuration));
  };
  static GetInstance = async (configuration: RedisConfiguration) => {
    const instance = new RedisBox(configuration);
    await instance.Initialize();
    return instance;
  };
  _configuration: RedisConfiguration;
  _inProcess: InProcess = {};
  // public event EventHandler<ExceptionEvent> ExceptionEvent = (sender, args) => { };
  Flush = async (): Promise<boolean> => {
    this._inProcess = {};
    try {
      await this.GetPooledConnection().Flush();
      return true;
    } catch (err) {
      this.RaiseException(err);
    }
    return false;
  };
  Reconnect = async (): Promise<boolean> => {
    this._inProcess = {};
    try {
      for (let index = 0; index < this.Pool.Connections.length; index++) {
        const connection = this.Pool.Connections[index];
        await connection.Reconnect();
      }
      return true;
    } catch (err) {
      this.RaiseException(err);
    }
    return false;
  };
  GetItem = async (key: string): Promise<?LitterBoxItem> => {
    if (!key) {
      // this.RaiseException(new ArgumentException($"{nameof(this.GetItem)}=>{nameof(key)} Cannot Be NullOrWhiteSpace"));
      return null;
    }

    try {
      const get = () =>
        new Promise((resolve, reject) => {
          this.GetPooledConnection().Cache.hget(key, 'litter', (err, value) => {
            if (err) {
              return reject(err);
            }
            if (value) {
              resolve(new LitterBoxItem(Compression.UnZip(value)));
            }
            resolve(null);
          });
        });
      return await get();
    } catch (err) {
      this.RaiseException(err);
    }

    return null;
  };
  Pool: ConnectionPool;
  GetPooledConnection = () => {
    return this.Pool.GetPooledConnection();
  };
  SetItem = async (key: string, original: LitterBoxItem): Promise<boolean> => {
    let success = false;
    if (!key) {
      // this.RaiseException(new ArgumentException($"{nameof(this.SetItem)}=>{nameof(key)} Cannot Be NullOrWhiteSpace"));
      return success;
    }
    if (!original) {
      // this.RaiseException(new ArgumentException($"{nameof(this.SetItem)}=>{nameof(original)} Cannot Be Null"));
      return success;
    }

    const litter = original.Clone();

    litter.CacheType = this.GetType();
    litter.Key = key;
    litter.TimeToRefresh = litter.TimeToRefresh || this._configuration.DefaultTimeToRefresh;
    litter.TimeToLive = litter.TimeToLive || this._configuration.DefaultTimeToLive;

    try {
      const item = Compression.Zip(litter);
      this.GetPooledConnection().Cache.hset(key, 'litter', item);
      this.GetPooledConnection().Cache.expire(key, litter.TimeToLive);
      success = true;
    } catch (err) {
      this.RaiseException(err);
    }

    return success;
  };
  SetItemFireAndForget = async (key: string, litter: LitterBoxItem): void => {
    if (!key) {
      // this.RaiseException(new ArgumentException($"{nameof(this.SetItemFireAndForget)}=>{nameof(key)} Cannot Be NullOrWhiteSpace"));
      return;
    }
    if (!litter) {
      // this.RaiseException(new ArgumentException($"{nameof(this.SetItemFireAndForget)}=>{nameof(litter)} Cannot Be Null"));
      return;
    }

    this._inProcess[key] = true;

    (async () => {
      await this.SetItem(key, litter);
    })();
  };
  SetItemFireAndForgetGenerated = async (
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

    this._inProcess[key] = true;

    (async () => {
      let toLive = null;
      let toRefresh = null;
      if (timeToLive != null) {
        toLive = timeToLive;
      }

      if (timeToRefresh != null) {
        toRefresh = timeToRefresh;
      }

      try {
        const item = await generator();
        if (item != null) {
          const litter = new LitterBoxItem({
            CacheType: this.GetType(),
            Key: key,
            Value: item,
            TimeToLive: toLive,
            TimeToRefresh: toRefresh
          });
          await this.SetItem(key, litter);
        }
      } catch (err) {
        this.RaiseException(err);
      }
    })();
  };
}
