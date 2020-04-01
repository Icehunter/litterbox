import { LitterBoxItem } from '../Models';

export interface IConnection {
  getItem(key: string): Promise<LitterBoxItem | null>;
  setItem(key: string, item: LitterBoxItem, timeToLive?: number, timeToRefresh?: number): Promise<boolean>;
  removeItem(key: string): Promise<boolean>;
  connect(): Promise<void>;
  flush(): Promise<boolean>;
  reconnect(): Promise<void>;
}
