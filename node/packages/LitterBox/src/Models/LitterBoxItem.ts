import { Compression } from '../Compression';
import { ILitterBoxItem } from '../Interfaces';
import { add } from 'date-fns';

export class LitterBoxItem<T> implements ILitterBoxItem<T> {
  constructor(props: ILitterBoxItem<T>) {
    const { cacheType = 'INITIAL', created = new Date(), key, timeToLive, timeToRefresh, value } = props;
    this.cacheType = cacheType;
    this.created = new Date(created);
    if (this.created.toString() === 'Invalid Date') {
      this.created = new Date();
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
  value: T;
  static fromBuffer = <T>(value: Buffer): LitterBoxItem<T> => Compression.inflate(value);
  static fromJSONString = <T>(value: string): LitterBoxItem<T> => new LitterBoxItem(JSON.parse(value));
  raw = (): ILitterBoxItem<T> => {
    const { cacheType, created, key, timeToLive, timeToRefresh, value } = this;
    return {
      cacheType,
      created,
      key,
      timeToLive,
      timeToRefresh,
      value
    };
  };
  toBuffer = (): Buffer => Compression.deflate(this.raw());
  toJSONString = (): string => JSON.stringify(this.raw());
  clone = (): LitterBoxItem<T> => {
    return new LitterBoxItem<T>(this.raw());
  };
  isExpired = (): boolean => {
    const { created, timeToLive } = this;
    if (timeToLive === undefined) {
      return false;
    }
    if (timeToLive === 0) {
      return true;
    }
    const comparisonDate = new Date(created.getTime());
    return (
      new Date().getTime() >
      add(comparisonDate, {
        seconds: timeToLive / 1000
      }).getTime()
    );
  };
  isStale = (): boolean => {
    const { created, timeToRefresh } = this;
    if (timeToRefresh === undefined) {
      return false;
    }
    if (timeToRefresh === 0) {
      return true;
    }
    const comparisonDate = new Date(created.getTime());
    return (
      new Date().getTime() >
      add(comparisonDate, {
        seconds: timeToRefresh / 1000
      }).getTime()
    );
  };
}
