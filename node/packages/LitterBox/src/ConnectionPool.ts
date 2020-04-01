import { IConnection, IConnectionPool } from './Interfaces';

interface IConnectionPoolProps {
  poolSize: number;
}

export class ConnectionPool implements IConnectionPool {
  constructor(props: IConnectionPoolProps) {
    const { poolSize = 5 } = props;
    this.poolSize = poolSize;
  }
  private _roundRobinCounter = 0;
  readonly connections: IConnection[] = [];
  readonly poolSize: number;
  getPooledConnection = (): IConnection => {
    return this.connections[this.incrementCount()];
  };
  initialize = async (connection: IConnection): Promise<void> => {
    for (let i = 0; i < this.poolSize; i++) {
      await connection.connect();
      this.connections.push(connection);
    }
  };
  incrementCount = (): number => {
    if (this._roundRobinCounter + 1 >= this.poolSize) {
      this._roundRobinCounter = 0;
    } else {
      this._roundRobinCounter++;
    }
    return this._roundRobinCounter;
  };
}
