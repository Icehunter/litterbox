import { ActionResult, LitterBoxItem } from '../Models';

export interface ITenancy {
  Flush(): Promise<ActionResult[]>;
  Reconnect(): Promise<ActionResult[]>;
  GetItem(key: string): Promise<LitterBoxItem | null>;
  GetItemUsingGenerator(
    key: string,
    generator: () => Promise<any>,
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<LitterBoxItem | null>;
  GetItems(keys: string[]): Promise<(LitterBoxItem | null)[]>;
  GetItemsUsingGenerator(
    keys: string[],
    generators: (() => Promise<any>)[],
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<(LitterBoxItem | null)[]>;
  SetItem(key: string, item: any, timeToLive?: number, timeToRefresh?: number): Promise<ActionResult[]>;
  SetItems(keys: string[], items: any, timeToLive?: number, timeToRefresh?: number): Promise<ActionResult[][]>;
  SetItemFireAndForget(key: string, item: any, timeToLive?: number, timeToRefresh?: number): void;
  SetItemFireAndForgetUsingGenerator(
    key: string,
    generator: () => Promise<any>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void;
  RemoveItem(key: string): Promise<ActionResult[]>;
  RemoveItems(keys: string[]): Promise<ActionResult[][]>;
}
