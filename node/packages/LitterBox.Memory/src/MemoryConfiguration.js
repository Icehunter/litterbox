// @flow

import { BaseConnectionConfiguration } from '@icehunter/litterbox';

export type MemoryConfigurationProps = {
  ExpirationScanFrequency: number
};

export class MemoryConfiguration extends BaseConnectionConfiguration {
  constructor(
    props: MemoryConfigurationProps = {
      ExpirationScanFrequency: 1 * 60 * 60 * 1000
    }
  ) {
    super(props);
    this.ExpirationScanFrequency = props.ExpirationScanFrequency;
  }
  ExpirationScanFrequency: number;
}
