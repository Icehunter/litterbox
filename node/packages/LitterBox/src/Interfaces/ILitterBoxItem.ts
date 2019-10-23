export interface ILitterBoxItem {
  CacheType?: string;
  Created?: Date | string;
  Key: string;
  TimeToLive?: number;
  TimeToRefresh?: number;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  Value: any;
}
