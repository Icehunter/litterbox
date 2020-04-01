import { IConnection } from './IConnection';

export interface IConnectionPool {
  getPooledConnection(): IConnection;
  initialize(connection: IConnection): Promise<void>;
  incrementCount(): number;
}
