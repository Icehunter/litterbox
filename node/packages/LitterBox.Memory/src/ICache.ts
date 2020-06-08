import { LitterBoxItem } from '@icehunter/litterbox';

export interface ICache {
  [key: string]: Buffer | LitterBoxItem<unknown>;
}
