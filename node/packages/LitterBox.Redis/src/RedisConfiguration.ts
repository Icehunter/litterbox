import { IRedisConfiguration } from './IRedisConfiguration';

type IRedisConfigurationProps = IRedisConfiguration;

export class RedisConfiguration implements IRedisConfiguration {
  constructor(props: IRedisConfigurationProps = {}) {
    const {
      defaultTimeToLive = 1 * 60 * 60 * 1000,
      defaultTimeToRefresh = 5 * 60 * 1000,
      useGZIPCompression = false,
      dataBaseID = 0,
      host = '127.0.0.1',
      password = '',
      port = 6379
    } = props;
    this.defaultTimeToLive = defaultTimeToLive;
    this.defaultTimeToRefresh = defaultTimeToRefresh;
    this.useGZIPCompression = useGZIPCompression;
    this.dataBaseID = dataBaseID;
    this.host = host;
    this.password = password;
    this.port = port;
  }
  defaultTimeToLive: number;
  defaultTimeToRefresh: number;
  useGZIPCompression: boolean;
  dataBaseID: number;
  host: string;
  password: string;
  port: number;
}
