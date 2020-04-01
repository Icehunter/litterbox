import { IBaseConnectionConfiguration } from '@icehunter/litterbox';

export interface IRedisConfiguration extends IBaseConnectionConfiguration {
  dataBaseID?: number;
  host?: string;
  password?: string;
  port?: number;
}
