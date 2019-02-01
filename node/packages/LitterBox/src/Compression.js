// @flow

import zlib from 'zlib';
import { LitterBoxItem } from './Models';

export class Compression {
  static UnZip = (input: Buffer): LitterBoxItem => {
    return JSON.parse(zlib.inflateSync(input).toString('utf8'));
  };
  static Zip = (litter: LitterBoxItem): Buffer => {
    const input = Buffer.from(JSON.stringify(litter), 'utf8');
    return zlib.deflateSync(input);
  };
}
