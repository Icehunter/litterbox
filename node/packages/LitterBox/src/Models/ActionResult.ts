import { IActionResult } from '../Interfaces';

type IActionResultProps = IActionResult;

export class ActionResult implements IActionResult {
  constructor(props: IActionResultProps) {
    const { cacheType, isSuccessful, error } = props;
    this.cacheType = cacheType;
    this.isSuccessful = isSuccessful ?? false;
    this.error = error ?? undefined;
  }
  cacheType: string;
  isSuccessful: boolean;
  error?: Error;
}
