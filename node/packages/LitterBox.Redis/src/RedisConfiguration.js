// @flow

import { BaseConnectionConfiguration } from '@icehunter/litterbox';

export type RedisConfigurationProps = {
  DataBaseID: number,
  Host: string,
  Password: string,
  Port: number
};

export class RedisConfiguration extends BaseConnectionConfiguration {
  constructor(
    props: RedisConfigurationProps = {
      DataBaseID: 0,
      Host: '127.0.0.1',
      Password: '',
      Port: 6379
    }
  ) {
    super(props);
    this.DataBaseID = props.DataBaseID;
    this.Host = props.Host;
    this.Password = props.Password;
    this.Port = props.Port;
  }
  DataBaseID: number;
  Host: string;
  Password: string;
  Port: number;
}
