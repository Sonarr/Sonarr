interface MediaInfo {
  videoBitDepth: number;
  videoBitrate: number;
  videoCodec: string;
  videoFps: number;
  videoDynamicRange: string;
  videoDynamicRangeType: string;
  resolution: string;
  runTime: string;
  scanType: string;
  audioStreams: MediaAudioStream[];
  subtitleStreams: MediaSubtitleStream[];
}

interface MediaStream {
  language: string;
}

interface MediaAudioStream extends MediaStream {
  codec: string;
  bitrate: number;
  channels: number;
  channelPositions: string;
  title: string;
}

interface MediaSubtitleStream extends MediaStream {
  format: string;
  title: string;
  forced?: boolean;
  hearingImpaired?: boolean;
}

export default MediaInfo;
