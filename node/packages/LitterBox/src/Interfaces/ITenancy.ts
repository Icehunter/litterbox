import { ActionResult, LitterBoxItem } from '../Models';

export interface ITenancy {
  Flush(): Promise<ActionResult[]>;
  Reconnect(): Promise<ActionResult[]>;
  GetItem(key: string): Promise<LitterBoxItem | null>;
  GetItemUsingGenerator(
    key: string,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    generator: () => Promise<any>,
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<LitterBoxItem | null>;
  GetItems(keys: string[]): Promise<(LitterBoxItem | null)[]>;
  GetItemsUsingGenerator(
    keys: string[],
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    generators: (() => Promise<any>)[],
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<(LitterBoxItem | null)[]>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  SetItem(key: string, item: any, timeToLive?: number, timeToRefresh?: number): Promise<ActionResult[]>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  SetItems(keys: string[], items: any, timeToLive?: number, timeToRefresh?: number): Promise<ActionResult[][]>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  SetItemFireAndForget(key: string, item: any, timeToLive?: number, timeToRefresh?: number): void;
  SetItemFireAndForgetUsingGenerator(
    key: string,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    generator: () => Promise<any>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void;
  RemoveItem(key: string): Promise<ActionResult[]>;
  RemoveItems(keys: string[]): Promise<ActionResult[][]>;
}
