import { IConnection, IConnectionPool } from './Interfaces';

interface IConnectionPoolProps extends IConnectionPool {}

export class ConnectionPool implements IConnectionPool {
  constructor(props: IConnectionPoolProps) {
    const { PoolSize = 5 } = props;
    this.PoolSize = PoolSize;
  }
  private _roundRobinCounter = 0;
  readonly Connections: IConnection[] = [];
  readonly PoolSize: number;
  GetPooledConnection = (): IConnection => {
    return this.Connections[this.IncrementCount()];
  };
  Initialize = async (connection: IConnection): Promise<void> => {
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
