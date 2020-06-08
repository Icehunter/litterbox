import { IBaseConnectionConfiguration, IConnection, IConnectionPool, ILitterBox, IProcessingCache } from './Interfaces';

import { ConnectionPool } from './ConnectionPool';
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
    this._type = type;
    this._pool = pool;
    this._configuration = configuration;
    this._connection = connection;
  }
  private _processingCache: IProcessingCache = {};
  private _type: string;
  private _pool: IConnectionPool;
  private _configuration: IBaseConnectionConfiguration;
  private _connection: IConnection;
  private _getPooledConnection = (): IConnection => {
    return this._pool.getPooledConnection();
  };
  initialize = async (): Promise<LitterBox> => {
    await this._pool.initialize(this._connection);
    return this;
  };
  getType = (): string => this._type;
  flush = async (): Promise<boolean> => {
    this._processingCache = {};
    try {
      this._getPooledConnection().flush();
      return true;
    } catch (err) {
      return false;
    }
  };
  reconnect = async (): Promise<boolean> => {
    this._processingCache = {};
    return true;
  };
  getItem = async <T>(key: string): Promise<LitterBoxItem<T> | null> => {
    try {
      const item = await this._getPooledConnection().getItem(key);
      if (item) {
        return item;
      }
    } catch (err) {
      console.error('GetItem', { key }, err);
    }
    return null;
  };
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  setItem = async <T>(
    key: string,
    item: LitterBoxItem<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<boolean> => {
    let success = false;
    try {
      success = await this._getPooledConnection().setItem(key, {
        ...item,
        cacheType: this.getType(),
        key,
        timeToLive: timeToLive || this._configuration.defaultTimeToLive,
        timeToRefresh: timeToRefresh || this._configuration.defaultTimeToRefresh
      });
    } catch (err) {
      console.error('SetItem', { key, item, timeToLive, timeToRefresh }, err);
    }
    Reflect.deleteProperty(this._processingCache, key);
    return success;
  };
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  setItemFireAndForget = <T>(
    key: string,
    item: LitterBoxItem<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void => {
    if (this._processingCache[key]) {
      return;
    }
    this._processingCache[key] = true;
    (async (): Promise<void> => {
      await this.setItem(key, item, timeToLive, timeToRefresh);
    })();
  };
  setItemFireAndForgetUsingGenerator = <T>(
    key: string,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    generator: () => Promise<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void => {
    if (this._processingCache[key]) {
      return;
    }
    this._processingCache[key] = true;
    (async (): Promise<void> => {
      const value = await generator();
      if (value != null) {
        await this.setItem(
          key,
          new LitterBoxItem({
            cacheType: this.getType(),
            key,
            timeToLive: timeToLive,
            timeToRefresh: timeToRefresh,
            value
          }),
          timeToLive,
          timeToRefresh
        );
      }
    })();
  };
  removeItem = async (key: string): Promise<boolean> => {
    return await this._getPooledConnection().removeItem(key);
  };
}
