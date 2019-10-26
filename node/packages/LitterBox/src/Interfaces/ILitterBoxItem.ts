export interface ILitterBoxItem {
  cacheType?: string;
  created?: Date | string;
  key: string;
  timeToLive?: number;
  timeToRefresh?: number;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  value: any;
}
