export interface IActionResult {
  CacheType: string;
  IsSuccessful?: boolean;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  Error?: any;
}
