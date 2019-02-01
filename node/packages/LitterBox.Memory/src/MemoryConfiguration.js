// @flow

import { BaseConnectionConfiguration } from 'litterbox';

export type MemoryConfigurationProps = {
  ExpirationScanFrequency: number
};

export class MemoryConfiguration extends BaseConnectionConfiguration {
  constructor(props: MemoryConfigurationProps = {}) {
    super(props);
    this.ExpirationScanFrequency = this.ExpirationScanFrequency || 1 * 60 * 60 * 1000;
  }
  ExpirationScanFrequency: number;
}
