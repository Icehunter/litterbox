import { ConnectionPool, LitterBox } from '@icehunter/litterbox';

import { MemoryConfiguration } from './MemoryConfiguration';
import { MemoryConnection } from './MemoryConnection';

export class MemoryBox {
  static getInstance = async (configuration: MemoryConfiguration): Promise<LitterBox> => {
    const instance = new LitterBox({
      type: 'LitterBox.Memory',
      pool: new ConnectionPool(configuration),
      configuration,
      connection: new MemoryConnection(configuration)
    });
    const box = await instance.initialize();
    return box;
  };
}
