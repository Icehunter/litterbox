import { LitterBoxItem } from '../Models';

export interface IConnection {
  getItem<T>(key: string): Promise<LitterBoxItem<T> | null>;
  setItem<T>(key: string, item: LitterBoxItem<T>, timeToLive?: number, timeToRefresh?: number): Promise<boolean>;
  removeItem(key: string): Promise<boolean>;
  connect(): Promise<void>;
  flush(): Promise<boolean>;
  reconnect(): Promise<void>;
}
