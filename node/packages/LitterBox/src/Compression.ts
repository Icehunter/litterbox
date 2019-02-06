import * as zlib from 'zlib';
import { LitterBoxItem } from './Models/LitterBoxItem';

export class Compression {
  static UnZip = (input: Buffer): LitterBoxItem => {
    return new LitterBoxItem(JSON.parse(zlib.inflateSync(input).toString('utf8')));
  };
  static Zip = (litter: LitterBoxItem): Buffer => {
    const input = Buffer.from(JSON.stringify(litter), 'utf8');
    return zlib.deflateSync(input);
  };
}
