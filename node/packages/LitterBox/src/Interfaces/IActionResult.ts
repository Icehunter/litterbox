export interface IActionResult {
  cacheType: string;
  isSuccessful?: boolean;
  error?: Error;
}
