// @flow

import { Compression, ConnectionPool, ILitterBox, LitterBoxItem } from 'litterbox';
import { MemoryConfiguration } from './MemoryConfiguration';
import { MemoryConnection } from './MemoryConnection';

type InProcess = {
  [index: string]: boolean
};

export class MemoryBox implements ILitterBox {
  constructor(configuration: MemoryConfiguration) {
    this._configuration = configuration;

    this.Pool = new ConnectionPool({
      PoolSize: 1
    });
  }
  GetType = (): string => 'LitterBox.Memory';
  RaiseException = (err: any) => {
    console.error(err);
    // this.ExceptionEvent?.Invoke(new ExceptionEvent(err));
  };
  Initialize = async () => {
    this.Pool.Initialize(new MemoryConnection(this._configuration));
  };
  static GetInstance = async (configuration: MemoryConfiguration) => {
    const instance = new MemoryBox(configuration);
    await instance.Initialize();
    return instance;
  };
  _configuration: MemoryConfiguration;
  _inProcess: InProcess = {};
  // public event EventHandler<ExceptionEvent> ExceptionEvent = (sender, args) => { };
  Flush = async (): Promise<boolean> => {
    this._inProcess = {};
    try {
      this.GetPooledConnection().Flush();
      return true;
    } catch (err) {
      this.RaiseException(err);
    }
    return false;
  };
  Reconnect = async (): Promise<boolean> => {
    this._inProcess = {};
    return true;
  };
  GetItem = async (key: string): Promise<?LitterBoxItem> => {
    if (!key) {
      // this.RaiseException(new ArgumentException($"{nameof(this.GetItem)}=>{nameof(key)} Cannot Be NullOrWhiteSpace"));
      return null;
    }

    try {
      const item = this.GetPooledConnection().Cache.Get(key);
      if (item) {
        const value = this._configuration.UseGZIPCompression ? Compression.UnZip(item) : JSON.parse(item);
        const litter = new LitterBoxItem(value);
        if (litter.IsExpired()) {
          this.GetPooledConnection().Cache.Delete(key);
          return null;
        }
        return litter;
      }
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
      const item = this._configuration.UseGZIPCompression ? Compression.Zip(litter) : JSON.stringify(litter);
      this.GetPooledConnection().Cache.Set(key, item);
      success = true;
    } catch (err) {
      this.RaiseException(err);
    }

    Reflect.deleteProperty(this._inProcess, key);

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

    if (this._inProcess[key]) {
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

    if (this._inProcess[key]) {
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
