import { IRedisConfiguration } from './IRedisConfiguration';

interface Props extends IRedisConfiguration {}

export class RedisConfiguration implements IRedisConfiguration {
  constructor(props: Props = {}) {
    const {
      DefaultTimeToLive = 1 * 60 * 60 * 1000,
      DefaultTimeToRefresh = 5 * 60 * 1000,
      PoolSize = 5,
      UseGZIPCompression = false,
      DataBaseID = 0,
      Host = '127.0.0.1',
      Password = '',
      Port = 6379
    } = props;
    this.DefaultTimeToLive = DefaultTimeToLive;
    this.DefaultTimeToRefresh = DefaultTimeToRefresh;
    this.PoolSize = PoolSize;
    this.UseGZIPCompression = UseGZIPCompression;
    this.DataBaseID = DataBaseID;
    this.Host = Host;
    this.Password = Password;
    this.Port = Port;
  }
  DefaultTimeToLive: number;
  DefaultTimeToRefresh: number;
  PoolSize: number;
  UseGZIPCompression: boolean;
  DataBaseID: number;
  Host: string;
  Password: string;
  Port: number;
}
