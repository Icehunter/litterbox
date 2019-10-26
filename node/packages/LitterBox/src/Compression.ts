import * as zlib from 'zlib';
import { LitterBoxItem } from './Models/LitterBoxItem';

export class Compression {
  static inflate = (input: Buffer): LitterBoxItem => {
    return new LitterBoxItem(JSON.parse(zlib.inflateSync(input).toString('utf8')));
  };
  static deflate = (litter: LitterBoxItem): Buffer => {
    return zlib.deflateSync(Buffer.from(JSON.stringify(litter)));
  };
}
