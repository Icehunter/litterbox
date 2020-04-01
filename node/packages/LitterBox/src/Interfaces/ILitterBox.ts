import { LitterBoxItem } from '../Models';

export interface ILitterBox {
  getType(): string;
  getItem(key: string): Promise<LitterBoxItem | null>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  setItem(key: string, item: any, timeToLive?: number, timeToRefresh?: number): Promise<boolean>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  setItemFireAndForget(key: string, item: any, timeToLive?: number, timeToRefresh?: number): void;
  setItemFireAndForgetUsingGenerator(
    key: string,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    generator: () => Promise<any>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void;
  removeItem(key: string): Promise<boolean>;
  flush(): Promise<boolean>;
  reconnect(): Promise<boolean>;
  initialize(): Promise<ILitterBox>;
}
