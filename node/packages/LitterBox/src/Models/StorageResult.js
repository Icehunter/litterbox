// @flow

import { ISuccessResult } from '../Interfaces';

export type StorageResultProps = {
  CacheType?: string,
  IsSuccessful?: boolean
};

export class StorageResult implements ISuccessResult {
  constructor(props: StorageResultProps = {}) {
    this.CacheType = props.CacheType || 'UNKNOWN_CACHE';
    this.IsSuccessful = props.IsSuccessful || false;
  }
  CacheType: string;
  IsSuccessful: boolean;
}
