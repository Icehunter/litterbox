import { ActionResult, LitterBoxItem } from '../Models';

import events from 'events';

export interface ITenancy {
  emitter(): events.EventEmitter;
  flush(): Promise<ActionResult[]>;
  reconnect(): Promise<ActionResult[]>;
  getItem<T>(key: string): Promise<LitterBoxItem<T> | undefined>;
  getItemUsingGenerator<T>(
    key: string,
    generator: () => Promise<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<LitterBoxItem<T> | undefined>;
  getItems<T>(keys: string[]): Promise<(LitterBoxItem<T> | undefined)[]>;
  getItemsUsingGenerator<T>(
    keys: string[],
    generators: (() => Promise<T>)[],
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<(LitterBoxItem<T> | undefined)[]>;
  setItem<T>(key: string, item: LitterBoxItem<T>, timeToLive?: number, timeToRefresh?: number): Promise<ActionResult[]>;
  setItems<T>(
    keys: string[],
    items: LitterBoxItem<T>[],
    timeToLive?: number,
    timeToRefresh?: number
  ): Promise<ActionResult[][]>;
  setItemFireAndForget<T>(key: string, item: LitterBoxItem<T>, timeToLive?: number, timeToRefresh?: number): void;
  setItemFireAndForgetUsingGenerator<T>(
    key: string,
    generator: () => Promise<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void;
  removeItem(key: string): Promise<ActionResult[]>;
  removeItems(keys: string[]): Promise<ActionResult[][]>;
}
