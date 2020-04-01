export interface IBaseConnectionConfiguration {
  defaultTimeToLive?: number;
  defaultTimeToRefresh?: number;
  poolSize?: number;
  useGZIPCompression?: boolean;
}
