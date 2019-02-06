import { LitterBox } from '../LitterBox';
import { LitterBoxItem } from '../Models';

export interface ILitterBox {
  GetType(): string;
  GetItem(key: string): Promise<LitterBoxItem | null>;
  SetItem(key: string, item: LitterBoxItem, timeToLive?: number, timeToRefresh?: number): Promise<boolean>;
  SetItemFireAndForget(key: string, item: any, timeToLive?: number, timeToRefresh?: number): void;
  SetItemFireAndForgetUsingGenerator(
    key: string,
    generator: () => Promise<any>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void;
  RemoveItem(key: string): Promise<boolean>;
  Flush(): Promise<boolean>;
  Reconnect(): Promise<boolean>;
  Initialize(): Promise<LitterBox>;
}
