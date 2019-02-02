// @flow

import { BaseConnectionConfiguration } from '@icehunter/litterbox';

export type RedisConfigurationProps = {
  DataBaseID?: number,
  Host?: string,
  Password?: string,
  Port?: number
};

export class RedisConfiguration extends BaseConnectionConfiguration {
  constructor(props: RedisConfigurationProps = {}) {
    super(props);
    this.DataBaseID = props.DataBaseID || 0;
    this.Host = props.Host || '127.0.0.1';
    this.Password = props.Password || '';
    this.Port = props.Port || 6380;
  }
  DataBaseID: number;
  Host: string;
  Password: string;
  Port: number;
}
