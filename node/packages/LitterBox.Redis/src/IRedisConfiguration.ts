import { IBaseConnectionConfiguration } from '@icehunter/litterbox';

export interface IRedisConfiguration extends IBaseConnectionConfiguration {
  DataBaseID?: number;
  Host?: string;
  Password?: string;
  Port?: number;
}
