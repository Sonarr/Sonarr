interface EpisodeSearchPayload {
  episodeId: number;
}

interface SeasonSearchPayload {
  seriesId: number;
  seasonNumber: number;
}

type InteractiveSearchPayload = EpisodeSearchPayload | SeasonSearchPayload;

export default InteractiveSearchPayload;
