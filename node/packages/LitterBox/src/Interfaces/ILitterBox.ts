import { LitterBoxItem } from '../Models';

export interface ILitterBox {
  getType(): string;
  getItem<T>(key: string): Promise<LitterBoxItem<T> | null>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  setItem<T>(key: string, item: LitterBoxItem<T>, timeToLive?: number, timeToRefresh?: number): Promise<boolean>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  setItemFireAndForget<T>(key: string, item: LitterBoxItem<T>, timeToLive?: number, timeToRefresh?: number): void;
  setItemFireAndForgetUsingGenerator<T>(
    key: string,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    generator: () => Promise<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void;
  removeItem(key: string): Promise<boolean>;
  flush(): Promise<boolean>;
  reconnect(): Promise<boolean>;
  initialize(): Promise<ILitterBox>;
}
