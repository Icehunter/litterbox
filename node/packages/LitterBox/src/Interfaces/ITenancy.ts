import { ActionResult, LitterBoxItem } from '../Models';

export interface ITenancy {
  flush(): Promise<ActionResult[]>;
  reconnect(): Promise<ActionResult[]>;
  getItem<T>(key: string): Promise<LitterBoxItem<T> | null>;
  getItemUsingGenerator<T>(
    key: string,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    generator: () => Promise<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<LitterBoxItem<T> | null>;
  getItems<T>(keys: string[]): Promise<(LitterBoxItem<T> | null)[]>;
  getItemsUsingGenerator<T>(
    keys: string[],
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    generators: (() => Promise<T>)[],
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<(LitterBoxItem<T> | null)[]>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  setItem<T>(key: string, item: LitterBoxItem<T>, timeToLive?: number, timeToRefresh?: number): Promise<ActionResult[]>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  setItems<T>(
    keys: string[],
    items: LitterBoxItem<T>[],
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<ActionResult[][]>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  setItemFireAndForget<T>(key: string, item: LitterBoxItem<T>, timeToLive?: number, timeToRefresh?: number): void;
  setItemFireAndForgetUsingGenerator<T>(
    key: string,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    generator: () => Promise<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void;
  removeItem(key: string): Promise<ActionResult[]>;
  removeItems(keys: string[]): Promise<ActionResult[][]>;
}
