import { ConnectionPool } from './ConnectionPool';
import { IBaseConnectionConfiguration, IConnection, IConnectionPool, ILitterBox, IProcessingCache } from './Interfaces';
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
  getItem = async (key: string): Promise<LitterBoxItem | null> => {
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
  setItem = async (key: string, item: any, timeToLive?: number, timeToRefresh?: number): Promise<boolean> => {
    let success = false;
    try {
      success = await this._getPooledConnection().setItem(
        key,
        new LitterBoxItem({
          cacheType: this.getType(),
          key,
          timeToLive: timeToLive || this._configuration.defaultTimeToLive,
          timeToRefresh: timeToRefresh || this._configuration.defaultTimeToRefresh,
          value: item
        })
      );
    } catch (err) {
      console.error('SetItem', { key, item, timeToLive, timeToRefresh }, err);
    }
    Reflect.deleteProperty(this._processingCache, key);
    return success;
  };
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  setItemFireAndForget = (key: string, item: any, timeToLive?: number, timeToRefresh?: number): void => {
    if (this._processingCache[key]) {
      return;
    }
    this._processingCache[key] = true;
    (async (): Promise<void> => {
      await this.setItem(key, item, timeToLive, timeToRefresh);
    })();
  };
  setItemFireAndForgetUsingGenerator = (
    key: string,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    generator: () => Promise<any>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void => {
    if (this._processingCache[key]) {
      return;
    }
    this._processingCache[key] = true;
    (async (): Promise<void> => {
      const item = await generator();
      if (item != null) {
        await this.setItem(
          key,
          new LitterBoxItem({
            cacheType: this.getType(),
            key,
            timeToLive: timeToLive,
            timeToRefresh: timeToRefresh,
            value: item
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
