import { LitterBoxItem } from '../Models';
import events from 'events';

export interface ILitterBox {
  emitter(): events.EventEmitter;
  getType(): string;
  getItem<T>(key: string): Promise<LitterBoxItem<T> | undefined>;
  setItem<T>(key: string, item: LitterBoxItem<T>, timeToLive?: number, timeToRefresh?: number): Promise<boolean>;
  setItemFireAndForget<T>(key: string, item: LitterBoxItem<T>, timeToLive?: number, timeToRefresh?: number): void;
  setItemFireAndForgetUsingGenerator<T>(
    key: string,
    generator: () => Promise<T>,
    timeToLive?: number,
    timeToRefresh?: number
  ): void;
  removeItem(key: string): Promise<boolean>;
  flush(): Promise<boolean>;
  reconnect(): Promise<boolean>;
  initialize(): Promise<ILitterBox>;
}
