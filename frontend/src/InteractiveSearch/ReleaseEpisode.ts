interface ReleaseEpisode {
  id: number;
  episodeFileId: number;
  seasonNumber: number;
  episodeNumber: number;
  absoluteEpisodeNumber?: number;
  title: string;
}

export default ReleaseEpisode;
