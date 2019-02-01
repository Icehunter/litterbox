// @flow

export type BaseConnectionConfigurationProps = {
  DefaultTimeToLive: number,
  DefaultTimeToRefresh: number,
  PoolSize: number,
  UseGZIPCompression: boolean
};

export class BaseConnectionConfiguration {
  constructor(props: BaseConnectionConfigurationProps = {}) {
    this.DefaultTimeToLive = props.DefaultTimeToLive || 1 * 60 * 60 * 1000;
    this.DefaultTimeToRefresh = props.DefaultTimeToRefresh || 5 * 1000;
    this.PoolSize = props.PoolSize || 5;
    this.UseGZIPCompression = props.UseGZIPCompression || false;
  }
  DefaultTimeToLive: number;
  DefaultTimeToRefresh: number;
  PoolSize: number;
  UseGZIPCompression: boolean;
}
