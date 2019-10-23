import { IActionResult } from '../Interfaces';

interface IActionResultProps extends IActionResult {}

export class ActionResult implements IActionResult {
  constructor(props: IActionResultProps) {
    if (props === null) {
      throw new Error('ArgumentException: (props === null) => props');
    }
    const { CacheType, IsSuccessful, Error: ResultError } = props;
    if (!CacheType) {
      throw new Error('ArgumentException: (!CacheType) => CacheType');
    }
    if (typeof CacheType !== 'string') {
      throw new Error("ArgumentException: (typeof CacheType !== 'string') => CacheType");
    }
    this.CacheType = CacheType;
    if (IsSuccessful) {
      if (typeof IsSuccessful !== 'boolean') {
        throw new Error("ArgumentException: (typeof IsSuccessful !== 'boolean') => IsSuccessful");
      }
      this.IsSuccessful = IsSuccessful;
    } else {
      this.IsSuccessful = false;
    }
    if (ResultError) {
      this.Error = ResultError;
    }
  }
  CacheType: string;
  IsSuccessful: boolean;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  Error: any;
}
