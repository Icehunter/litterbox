// @flow

import { ISuccessResult } from '../Interfaces';

export type FlushResultProps = {
  CacheType?: string,
  IsSuccessful?: boolean
};

export class FlushResult implements ISuccessResult {
  constructor(props: FlushResultProps = {}) {
    this.CacheType = props.CacheType || 'UNKNOWN_CACHE';
    this.IsSuccessful = props.IsSuccessful || false;
  }
  CacheType: string;
  IsSuccessful: boolean;
}
