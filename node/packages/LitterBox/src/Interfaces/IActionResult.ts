export interface IActionResult {
  cacheType: string;
  isSuccessful?: boolean;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  error?: any;
}
