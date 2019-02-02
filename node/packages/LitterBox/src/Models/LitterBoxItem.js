// @flow

export type LitterBoxItemProps = {
  CacheType?: string,
  Created?: Date,
  Key?: string,
  TimeToLive?: ?number,
  TimeToRefresh?: ?number,
  Value?: any
};

export class LitterBoxItem {
  constructor(props: LitterBoxItemProps = {}) {
    this.CacheType = props.CacheType || 'UNKNOWN_CACHE';
    if (props.Created) {
      if (props.Created instanceof Date) {
        this.Created = props.Created;
      }
      if (props.Created instanceof String) {
        this.Created = new Date(props.Created.toString());
      }
    }
    this.Key = props.Key || 'UNKNOWN_KEY';
    this.TimeToLive = props.TimeToLive;
    this.TimeToRefresh = props.TimeToRefresh;
    this.Value = props.Value;
  }
  CacheType: string;
  Created: Date = new Date();
  Key: string;
  TimeToLive: ?number;
  TimeToRefresh: ?number;
  Value: any;
  Clone = (): LitterBoxItem => {
    return new LitterBoxItem({
      CacheType: this.CacheType,
      Created: this.Created,
      Key: this.Key,
      TimeToLive: this.TimeToLive,
      TimeToRefresh: this.TimeToRefresh
    });
  };
  IsExpired = (): boolean => {
    if (this.TimeToLive === null) {
      return false;
    }
    return new Date() > this.Created.setSeconds(this.Created.getSeconds() + this.TimeToLive);
  };
  IsStale = (): boolean => {
    if (this.TimeToRefresh === null) {
      return false;
    }
    return new Date() > this.Created.setSeconds(this.Created.getSeconds() + this.TimeToRefresh);
  };
}
