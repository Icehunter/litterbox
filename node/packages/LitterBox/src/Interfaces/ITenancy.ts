import { ActionResult, LitterBoxItem } from '../Models';

export interface ITenancy {
  flush(): Promise<ActionResult[]>;
  reconnect(): Promise<ActionResult[]>;
  getItem(key: string): Promise<LitterBoxItem | null>;
  getItemUsingGenerator(
    key: string,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    generator: () => Promise<any>,
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<LitterBoxItem | null>;
  getItems(keys: string[]): Promise<(LitterBoxItem | null)[]>;
  getItemsUsingGenerator(
    keys: string[],
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    generators: (() => Promise<any>)[],
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<(LitterBoxItem | null)[]>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  setItem(key: string, item: any, timeToLive?: number, timeToRefresh?: number): Promise<ActionResult[]>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  setItems(keys: string[], items: any, timeToLive?: number, timeToRefresh?: number): Promise<ActionResult[][]>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  setItemFireAndForget(key: string, item: any, timeToLive?: number, timeToRefresh?: number): void;
  setItemFireAndForgetUsingGenerator(
    key: string,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    generator: () => Promise<any>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void;
  removeItem(key: string): Promise<ActionResult[]>;
  removeItems(keys: string[]): Promise<ActionResult[][]>;
}
