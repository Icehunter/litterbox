// @flow

export type CacheItemProps = {
  Expiry?: number,
  Value?: LitterBoxItem
};

export class CacheItem {
  constructor(props: CacheItemProps = {}) {
    this.Expiry = props.Expiry;
    this.Value = props.Value;
  }
  Created: Date = new Date();
  Expiry: number;
  Value: ?LitterBoxItem = null;
  IsExpired = (): boolean => {
    if (this.Expiry === null) {
      return false;
    }
    return new Date() > this.Created.setSeconds(this.Created.getSeconds() + this.Expiry);
  };
}
