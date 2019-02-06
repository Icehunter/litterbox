import { ConnectionPool, LitterBox } from '@icehunter/litterbox';
import { MemoryConfiguration } from './MemoryConfiguration';
import { MemoryConnection } from './MemoryConnection';

export class MemoryBox {
  constructor(configuration: MemoryConfiguration) {
    this._configuration = configuration;
  }
  static GetInstance = async (configuration: MemoryConfiguration): Promise<LitterBox> => {
    const instance = new LitterBox({
      type: 'LitterBox.Memory',
      pool: new ConnectionPool(configuration),
      configuration,
      connection: new MemoryConnection(configuration)
    });
    return await instance.Initialize();
  };
  _configuration: MemoryConfiguration;
}
