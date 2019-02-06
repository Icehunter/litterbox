export interface ILitterBoxItem {
  CacheType?: string;
  Created?: Date | string;
  Key: string;
  TimeToLive?: number;
  TimeToRefresh?: number;
  Value: any;
}
