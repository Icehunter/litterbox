import { ConnectionPool } from './ConnectionPool';
import { IBaseConnectionConfiguration, IConnection, ILitterBox, IProcessingCache } from './Interfaces';
import { LitterBoxItem } from './Models';

interface ILitterBoxProps {
  type: string;
  pool: ConnectionPool;
  configuration: IBaseConnectionConfiguration;
  connection: IConnection;
}

export class LitterBox implements ILitterBox {
  constructor(props: ILitterBoxProps) {
    const { type, pool, configuration, connection } = props;

    if (!type) {
      throw new Error(`ArgumentException: (null | undefined) => type`);
    }
    if (!pool) {
      throw new Error(`ArgumentException: (null | undefined) => pool`);
    }
    if (!configuration) {
      throw new Error(`ArgumentException: (null | undefined) => configuration`);
    }
    if (!connection) {
      throw new Error(`ArgumentException: (null | undefined) => connection`);
    }
    this._type = type;
    this._pool = pool;
    this._configuration = configuration;
    this._connection = connection;
  }
  _processingCache: IProcessingCache = {};
  _type: string;
  _pool: ConnectionPool;
  _configuration: IBaseConnectionConfiguration;
  _connection: IConnection;
  _getPooledConnection = (): IConnection => {
    return this._pool.GetPooledConnection();
  };
  Initialize = async (): Promise<LitterBox> => {
    await this._pool.Initialize(this._connection);
    return this;
  };
  GetType = (): string => this._type;
  Flush = async (): Promise<boolean> => {
    this._processingCache = {};
    try {
      this._getPooledConnection().Flush();
      return true;
    } catch (err) {
      return false;
    }
  };
  Reconnect = async (): Promise<boolean> => {
    this._processingCache = {};
    return true;
  };
  GetItem = async (key: string): Promise<LitterBoxItem | null> => {
    if (!key) {
      throw new Error(`ArgumentException: (null | undefined) => key`);
    }
    try {
      const item = await this._getPooledConnection().GetItem(key);
      if (item) {
        return item;
      }
    } catch (err) {
      console.error('GetItem', { key }, err);
    }
    return null;
  };
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  SetItem = async (key: string, item: any, timeToLive?: number, timeToRefresh?: number): Promise<boolean> => {
    if (!key) {
      throw new Error(`ArgumentException: (null | undefined) => key`);
    }
    if (!item) {
      throw new Error(`ArgumentException: (null | undefined) => item`);
    }
    let success = false;
    try {
      success = await this._getPooledConnection().SetItem(
        key,
        new LitterBoxItem({
          CacheType: this.GetType(),
          Key: key,
          TimeToLive: timeToLive || this._configuration.DefaultTimeToLive,
          TimeToRefresh: timeToRefresh || this._configuration.DefaultTimeToRefresh,
          Value: item
        })
      );
    } catch (err) {
      console.error('SetItem', { key, item, timeToLive, timeToRefresh }, err);
    }
    Reflect.deleteProperty(this._processingCache, key);
    return success;
  };
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  SetItemFireAndForget = (key: string, item: any, timeToLive?: number, timeToRefresh?: number): void => {
    if (!key) {
      throw new Error(`ArgumentException: (null | undefined) => key`);
    }
    if (!item) {
      throw new Error(`ArgumentException: (null | undefined) => item`);
    }
    if (this._processingCache[key]) {
      return;
    }
    this._processingCache[key] = true;
    (async (): Promise<void> => {
      await this.SetItem(key, item, timeToLive, timeToRefresh);
    })();
  };
  SetItemFireAndForgetUsingGenerator = (
    key: string,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    generator: () => Promise<any>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void => {
    if (!key) {
      throw new Error(`ArgumentException: (null | undefined) => key`);
    }
    if (!generator) {
      throw new Error(`ArgumentException: (null | undefined) => generator`);
    }
    if (this._processingCache[key]) {
      return;
    }
    this._processingCache[key] = true;
    (async (): Promise<void> => {
      const item = await generator();
      if (item != null) {
        await this.SetItem(
          key,
          new LitterBoxItem({
            CacheType: this.GetType(),
            Key: key,
            Value: item,
            TimeToLive: timeToLive,
            TimeToRefresh: timeToRefresh
          }),
          timeToLive,
          timeToRefresh
        );
      }
    })();
  };
  RemoveItem = async (key: string): Promise<boolean> => {
    return await this._getPooledConnection().RemoveItem(key);
  };
}
