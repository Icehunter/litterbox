import { LitterBoxItem } from '../Models';

export interface IConnection {
  GetItem(key: string): Promise<LitterBoxItem | null>;
  SetItem(key: string, item: LitterBoxItem, timeToLive?: number, timeToRefresh?: number): Promise<boolean>;
  RemoveItem(key: string): Promise<boolean>;
  Connect(): Promise<void>;
  Flush(): Promise<boolean>;
  Reconnect(): Promise<void>;
}
