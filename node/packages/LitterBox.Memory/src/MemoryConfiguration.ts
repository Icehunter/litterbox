import { IMemoryConfiguration } from './IMemoryConfiguration';

type IMemoryConfigurationProps = IMemoryConfiguration;

export class MemoryConfiguration implements IMemoryConfiguration {
  constructor(props: IMemoryConfigurationProps = {}) {
    const {
      defaultTimeToLive = 1 * 60 * 60 * 1000,
      defaultTimeToRefresh = 60 * 1000,
      poolSize = 1,
      useGZIPCompression = false,
      expirationScanFrequency = 30 * 1000
    } = props;
    this.defaultTimeToLive = defaultTimeToLive;
    this.defaultTimeToRefresh = defaultTimeToRefresh;
    this.poolSize = poolSize;
    this.useGZIPCompression = useGZIPCompression;
    this.expirationScanFrequency = expirationScanFrequency;
  }
  defaultTimeToLive: number;
  defaultTimeToRefresh: number;
  poolSize: number;
  useGZIPCompression: boolean;
  expirationScanFrequency: number;
}
