// @flow

export class ExceptionEvent {
  constructor(error: any) {
    this.error = error;
  }
  error: any;
}
