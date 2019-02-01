// @flow

export class BaseConnectionConfiguration {
  DefaultTimeToLive: number = 1 * 60 * 60 * 1000;
  DefaultTimeToRefresh: number = 5 * 1000;
  PoolSize: number = 5;
}
