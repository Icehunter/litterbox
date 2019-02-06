import { Compression } from '../Compression';
import { ILitterBoxItem } from '../Interfaces';

interface Props extends ILitterBoxItem {}

export class LitterBoxItem {
  constructor({
    CacheType = 'UNKNOWN_CACHE',
    Created,
    Key = 'UNKNOWN_KEY',
    TimeToLive,
    TimeToRefresh,
    Value = null
  }: Props) {
    this.CacheType = CacheType;
    this.Created = new Date();
    if (Created instanceof Date) {
      this.Created = Created;
    }
    if (typeof Created === 'string') {
      this.Created = new Date(Created);
    }
    this.Key = Key;
    this.TimeToLive = TimeToLive;
    this.TimeToRefresh = TimeToRefresh;
    this.Value = Value;
  }
  CacheType: string;
  Created: Date;
  Key: string;
  TimeToLive?: number;
  TimeToRefresh?: number;
  Value: any;
  static FromBuffer = (value: Buffer): LitterBoxItem => Compression.UnZip(value);
  static FromJSONString = (value: string): LitterBoxItem => new LitterBoxItem(JSON.parse(value));
  ToBuffer = (): Buffer => Compression.Zip(this);
  ToJSONString = (): string => JSON.stringify(this);
  Clone = (): LitterBoxItem => {
    const { CacheType, Created, Key, TimeToLive, TimeToRefresh, Value } = this;
    return new LitterBoxItem({
      CacheType,
      Created,
      Key,
      TimeToLive,
      TimeToRefresh,
      Value
    });
  };
  IsExpired = (): boolean => {
    const { Created, TimeToLive } = this;
    if (TimeToLive === null) {
      return false;
    }
    return new Date().getTime() > Created.setSeconds(Created.getSeconds() + (TimeToLive || 0));
  };
  IsStale = (): boolean => {
    const { Created, TimeToRefresh } = this;
    if (TimeToRefresh === null) {
      return false;
    }
    return new Date().getTime() > Created.setSeconds(Created.getSeconds() + (TimeToRefresh || 0));
  };
}
