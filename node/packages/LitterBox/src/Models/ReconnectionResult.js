// @flow

import { ISuccessResult } from '../Interfaces';

export type ReconnectionResultProps = {
  CacheType?: string,
  IsSuccessful?: boolean
};

export class ReconnectionResult implements ISuccessResult {
  constructor(props: ReconnectionResultProps = {}) {
    this.CacheType = props.CacheType || 'UNKNOWN_CACHE';
    this.IsSuccessful = props.IsSuccessful || false;
  }
  CacheType: string;
  IsSuccessful: boolean;
}
