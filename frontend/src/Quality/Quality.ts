export enum QualitySource {
  Unknown = 'unknown',
  Television = 'television',
  TelevisionRaw = 'televisionRaw',
  Web = 'web',
  WebRip = 'webRip',
  DVD = 'dvd',
  Bluray = 'bluray',
  BlurayRaw = 'blurayRaw',
}

export interface Revision {
  version: number;
  real: number;
  isRepack: boolean;
}

interface Quality {
  id: number;
  name: string;
  resolution: number;
  source: QualitySource;
}

export interface QualityModel {
  quality: Quality;
  revision: Revision;
}

export default Quality;
