// @flow

import { ConnectionPool } from '../ConnectionPool';
import { LitterBoxItem } from '../Models';

export interface ILitterBox {
  // event EventHandler<ExceptionEvent> ExceptionEvent;
  GetItem(key: string): Promise<?LitterBoxItem>;
  SetItem(key: string, litter: LitterBoxItem): Promise<boolean>;
  Pool: ConnectionPool;
  GetPooledConnection(): ConnectionPool;
  Flush(): Promise<boolean>;
  Reconnect(): Promise<boolean>;
  GetType(): string;
  SetItemFireAndForget(key: string, litter: LitterBoxItem): void;
  SetItemFireAndForgetGenerated(
    key: string,
    generator: () => Promise<any>,
    timeToRefresh: number,
    timeToLive: number
  ): void;
}
