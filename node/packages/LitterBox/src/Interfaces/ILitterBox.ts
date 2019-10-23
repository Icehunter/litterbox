import { LitterBox } from '../LitterBox';
import { LitterBoxItem } from '../Models';

export interface ILitterBox {
  GetType(): string;
  GetItem(key: string): Promise<LitterBoxItem | null>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  SetItem(key: string, item: any, timeToLive?: number, timeToRefresh?: number): Promise<boolean>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  SetItemFireAndForget(key: string, item: any, timeToLive?: number, timeToRefresh?: number): void;
  SetItemFireAndForgetUsingGenerator(
    key: string,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    generator: () => Promise<any>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void;
  RemoveItem(key: string): Promise<boolean>;
  Flush(): Promise<boolean>;
  Reconnect(): Promise<boolean>;
  Initialize(): Promise<LitterBox>;
}
