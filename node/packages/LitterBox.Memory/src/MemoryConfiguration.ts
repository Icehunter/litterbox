import { IMemoryConfiguration } from './IMemoryConfiguration';

interface IMemoryConfigurationProps extends IMemoryConfiguration {}
export class MemoryConfiguration implements IMemoryConfiguration {
  constructor(props: IMemoryConfigurationProps = {}) {
    const {
      DefaultTimeToLive = 1 * 60 * 60 * 1000,
      DefaultTimeToRefresh = 60 * 1000,
      PoolSize = 1,
      UseGZIPCompression = false,
      ExpirationScanFrequency = 30 * 1000
    } = props;
    this.DefaultTimeToLive = DefaultTimeToLive;
    this.DefaultTimeToRefresh = DefaultTimeToRefresh;
    this.PoolSize = PoolSize;
    this.UseGZIPCompression = UseGZIPCompression;
    this.ExpirationScanFrequency = ExpirationScanFrequency;
  }
  DefaultTimeToLive: number;
  DefaultTimeToRefresh: number;
  PoolSize: number;
  UseGZIPCompression: boolean;
  ExpirationScanFrequency: number;
}
