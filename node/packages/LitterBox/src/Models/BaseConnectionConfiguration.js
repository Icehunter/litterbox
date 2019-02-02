// @flow

export type BaseConnectionConfigurationProps = {
  DefaultTimeToLive: number,
  DefaultTimeToRefresh: number,
  PoolSize: number,
  UseGZIPCompression: boolean
};

export class BaseConnectionConfiguration {
  constructor(
    props: BaseConnectionConfigurationProps = {
      DefaultTimeToLive: 1 * 60 * 60 * 1000,
      DefaultTimeToRefresh: 5 * 1000,
      PoolSize: 5,
      UseGZIPCompression: false
    }
  ) {
    this.DefaultTimeToLive = props.DefaultTimeToLive;
    this.DefaultTimeToRefresh = props.DefaultTimeToRefresh;
    this.PoolSize = props.PoolSize;
    this.UseGZIPCompression = props.UseGZIPCompression;
  }
  DefaultTimeToLive: number;
  DefaultTimeToRefresh: number;
  PoolSize: number;
  UseGZIPCompression: boolean;
}
