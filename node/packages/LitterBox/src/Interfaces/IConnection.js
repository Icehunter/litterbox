// @flow

export interface IConnection {
  Connect(): Promise<void>;
  Flush(): Promise<boolean>;
  Reconnect(): Promise<void>;
}
