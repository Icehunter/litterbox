// @flow

import { FlushResult, LitterBoxItem, ReconnectionResult, StorageResult } from '../Models';

export interface ITenancy {
  // event EventHandler<ExceptionEvent> ExceptionEvent;
  Flush(): Promise<?((?FlushResult)[])>;
  Reconnect(): Promise<?((?ReconnectionResult)[])>;
  GetItem(key: string): Promise<?LitterBoxItem>;
  GetItemGenerated(
    key: string,
    generator: () => Promise<any>,
    timeToRefresh: number,
    timeToLive: number
  ): Promise<?LitterBoxItem>;
  GetItems(keys: string[]): Promise<?((?LitterBoxItem)[])>;
  GetItemsGenerated(
    keys: string[],
    generators: (() => Promise<any>)[],
    timeToRefresh: number,
    timeToLive: number
  ): Promise<?((?LitterBoxItem)[])>;
  SetItem(key: string, litter: LitterBoxItem): Promise<?((?StorageResult)[])>;
  SetItems(keys: string[], litters: LitterBoxItem[]): Promise<?((?((?StorageResult)[]))[])>;
  SetItemFireAndForget(key: string, litter: LitterBoxItem): void;
  SetItemFireAndForgetGenerated(
    key: string,
    generator: () => Promise<any>,
    timeToRefresh: number,
    timeToLive: number
  ): void;
}
