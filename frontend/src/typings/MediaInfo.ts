interface MediaInfo {
  audioBitrate: number;
  audioChannels: number;
  audioCodec: string;
  audioLanguages: string;
  audioStreamCount: number;
  videoBitDepth: number;
  videoBitrate: number;
  videoCodec: string;
  videoFps: number;
  videoDynamicRange: string;
  videoDynamicRangeType: string;
  resolution: string;
  runTime: string;
  scanType: string;
  subtitles: string;
}

export default MediaInfo;
