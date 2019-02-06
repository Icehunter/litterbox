import { IBaseConnectionConfiguration } from '@icehunter/litterbox';

export interface IMemoryConfiguration extends IBaseConnectionConfiguration {
  ExpirationScanFrequency?: number;
}
