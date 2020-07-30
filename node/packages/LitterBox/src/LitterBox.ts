import { IBaseConnectionConfiguration, IConnection, IConnectionPool, ILitterBox, IProcessingCache } from './Interfaces';

import { ConnectionPool } from './ConnectionPool';
import { LitterBoxEvents } from './events';
import { LitterBoxItem } from './Models';
import events from 'events';

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
    this._emitter = new events.EventEmitter();
  }
  private _processingCache: IProcessingCache = {};
  private _type: string;
  private _pool: IConnectionPool;
  private _configuration: IBaseConnectionConfiguration;
  private _connection: IConnection;
  private _emitter: events.EventEmitter;
  private _getPooledConnection = (): IConnection => {
    return this._pool.getPooledConnection();
  };
  emitter = (): events.EventEmitter => this._emitter;
  initialize = async (): Promise<LitterBox> => {
    await this._pool.initialize(this._connection);
    return this;
  };
  getType = (): string => this._type;
  flush = async (): Promise<boolean> => {
    this._processingCache = {};
    this._getPooledConnection().flush();
    return true;
  };
  reconnect = async (): Promise<boolean> => {
    this._processingCache = {};
    return true;
  };
  getItem = async <T>(key: string): Promise<LitterBoxItem<T> | undefined> => {
    const item = await this._getPooledConnection().getItem<T>(key);
    return item;
  };
  setItem = async <T>(
    key: string,
    item: LitterBoxItem<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<boolean> => {
    const { created, value } = item;
    const litter = new LitterBoxItem<T>({
      key,
      created,
      value,
      cacheType: this.getType(),
      timeToLive: timeToLive ?? item.timeToLive ?? this._configuration.defaultTimeToLive,
      timeToRefresh: timeToRefresh ?? item.timeToRefresh ?? this._configuration.defaultTimeToRefresh
    });
    const wasSet = await this._getPooledConnection().setItem<T>(key, litter, timeToLive, timeToRefresh);
    return wasSet;
  };
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
      this.setItem(key, item, timeToLive, timeToRefresh)
        .catch((err) => {
          this._emitter.emit(LitterBoxEvents.errors.FIRE_FORGET, {
            type: this._type,
            key,
            err
          });
        })
        .finally(() => {
          Reflect.deleteProperty(this._processingCache, key);
        });
    })();
  };
  setItemFireAndForgetUsingGenerator = <T>(
    key: string,
    generator: () => Promise<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void => {
    if (this._processingCache[key]) {
      return;
    }
    this._processingCache[key] = true;
    (async (): Promise<void> => {
      generator()
        .then((value) =>
          this.setItem(
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
          )
        )
        .catch((err) => {
          this._emitter.emit(LitterBoxEvents.errors.FIRE_FORGET_USING_GENERATOR, {
            type: this._type,
            key,
            err
          });
        })
        .finally(() => {
          Reflect.deleteProperty(this._processingCache, key);
        });
    })();
  };
  removeItem = async (key: string): Promise<boolean> => {
    const wasRemoved = await this._getPooledConnection().removeItem(key);
    return wasRemoved;
  };
}
