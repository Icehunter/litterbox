// @flow

import { BaseConnectionConfiguration } from '@icehunter/litterbox';

export type MemoryConfigurationProps = {
  ExpirationScanFrequency: number
};

export class MemoryConfiguration extends BaseConnectionConfiguration {
  constructor(
    props: MemoryConfigurationProps = {
      ExpirationScanFrequency: 30 * 1000
    }
  ) {
    super(props);
    this.ExpirationScanFrequency = props.ExpirationScanFrequency;
  }
  ExpirationScanFrequency: number;
}
