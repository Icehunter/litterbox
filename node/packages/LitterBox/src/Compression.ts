import * as zlib from 'zlib';

import { ILitterBoxItem } from './Interfaces';
import { LitterBoxItem } from './Models';

export class Compression {
  static inflate = <T>(input: Buffer): LitterBoxItem<T> => {
    return new LitterBoxItem(JSON.parse(zlib.inflateSync(input).toString('utf8')));
  };
  static deflate = <T>(litter: ILitterBoxItem<T>): Buffer => {
    return zlib.deflateSync(Buffer.from(JSON.stringify(litter)));
  };
}
