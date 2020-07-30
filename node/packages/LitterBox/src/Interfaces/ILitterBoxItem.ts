export interface ILitterBoxItem<T> {
  cacheType?: string;
  created?: Date;
  key: string;
  timeToLive?: number;
  timeToRefresh?: number;
  value: T;
}
