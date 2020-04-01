import { Compression } from '../Compression';
import { ILitterBoxItem } from '../Interfaces';

interface ILitterBoxItemProps extends ILitterBoxItem {}

export class LitterBoxItem implements ILitterBoxItem {
  constructor(props: ILitterBoxItemProps) {
    const {
      cacheType = 'UNKNOWN_CACHE',
      created,
      key = 'UNKNOWN_KEY',
      timeToLive,
      timeToRefresh,
      value = null
    } = props;
    this.cacheType = cacheType;
    this.created = new Date();
    if (created instanceof Date) {
      this.created = created;
    }
    if (typeof created === 'string') {
      this.created = new Date(created);
    }
    this.key = key;
    this.timeToLive = timeToLive;
    this.timeToRefresh = timeToRefresh;
    this.value = value;
  }
  cacheType: string;
  created: Date;
  key: string;
  timeToLive?: number;
  timeToRefresh?: number;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  value: any;
  static fromBuffer = (value: Buffer): LitterBoxItem => Compression.inflate(value);
  static fromJSONString = (value: string): LitterBoxItem => new LitterBoxItem(JSON.parse(value));
  toBuffer = (): Buffer => Compression.deflate(this);
  toJSONString = (): string => JSON.stringify(this);
  clone = (): LitterBoxItem => {
    const { cacheType, created, key, timeToLive, timeToRefresh, value } = this;
    return new LitterBoxItem({
      cacheType,
      created,
      key,
      timeToLive,
      timeToRefresh,
      value
    });
  };
  isExpired = (): boolean => {
    const { created, timeToLive } = this;
    if (timeToLive === null) {
      return false;
    }
    return new Date().getTime() > created.setSeconds(created.getSeconds() + (timeToLive || 0));
  };
  isStale = (): boolean => {
    const { created, timeToRefresh } = this;
    if (timeToRefresh === null) {
      return false;
    }
    return new Date().getTime() > created.setSeconds(created.getSeconds() + (timeToRefresh || 0));
  };
}
