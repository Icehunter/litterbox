// @flow

import { IConnection } from './Interfaces';

export type ConnectionPoolProps = {
  PoolSize: number
};

export class ConnectionPool {
  constructor(props: ConnectionPoolProps) {
    this.PoolSize = props.PoolSize;
  }
  _roundRobinCounter: number = 0;
  Connections: IConnection[] = [];
  PoolSize: number = 5;
  GetPooledConnection = (): IConnection => {
    return this.Connections[this.IncrementCount()];
  };
  Initialize = async (connection: IConnection) => {
    for (let i = 0; i < this.PoolSize; i++) {
      await connection.Connect();
      this.Connections.push(connection);
    }
  };
  IncrementCount = (): number => {
    if (this._roundRobinCounter + 1 >= this.PoolSize) {
      this._roundRobinCounter = 0;
    } else {
      this._roundRobinCounter++;
    }
    return this._roundRobinCounter;
  };
}
