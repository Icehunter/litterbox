import { Compression } from '../Compression';
import { ILitterBoxItem } from '../Interfaces';

export class LitterBoxItem<T> implements ILitterBoxItem<T> {
  constructor(props: ILitterBoxItem<T>) {
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
  static fromBuffer = <T>(value: Buffer): LitterBoxItem<T> => Compression.inflate(value);
  static fromJSONString = <T>(value: string): LitterBoxItem<T> => new LitterBoxItem(JSON.parse(value));
  toBuffer = (): Buffer => Compression.deflate(this);
  toJSONString = (): string => JSON.stringify(this);
  clone = (): LitterBoxItem<T> => {
    const { cacheType, created, key, timeToLive, timeToRefresh, value } = this;
    return new LitterBoxItem<T>({
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
    const comparisonDate = new Date(created.getTime());
    return new Date().getTime() > comparisonDate.setSeconds(comparisonDate.getSeconds() + (timeToLive || 0));
  };
  isStale = (): boolean => {
    const { created, timeToRefresh } = this;
    if (timeToRefresh === null) {
      return false;
    }
    const comparisonDate = new Date(created.getTime());
    return new Date().getTime() > comparisonDate.setSeconds(comparisonDate.getSeconds() + (timeToRefresh || 0));
  };
}
