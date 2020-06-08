import { IActionResult } from '../Interfaces';

type IActionResultProps = IActionResult;

export class ActionResult implements IActionResult {
  constructor(props: IActionResultProps) {
    const { cacheType, isSuccessful, error: ResultError } = props;
    this.cacheType = cacheType;
    if (isSuccessful) {
      if (typeof isSuccessful !== 'boolean') {
        throw new Error("ArgumentException: (typeof isSuccessful !== 'boolean') => isSuccessful");
      }
      this.isSuccessful = isSuccessful;
    } else {
      this.isSuccessful = false;
    }
    if (ResultError) {
      this.error = ResultError;
    }
  }
  cacheType: string;
  isSuccessful: boolean;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  error: any;
}
