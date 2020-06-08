import * as zlib from 'zlib';

import { LitterBoxItem } from './Models/LitterBoxItem';

export class Compression {
  static inflate = <T>(input: Buffer): LitterBoxItem<T> => {
    return new LitterBoxItem(JSON.parse(zlib.inflateSync(input).toString('utf8')));
  };
  static deflate = <T>(litter: LitterBoxItem<T>): Buffer => {
    return zlib.deflateSync(Buffer.from(JSON.stringify(litter)));
  };
}
