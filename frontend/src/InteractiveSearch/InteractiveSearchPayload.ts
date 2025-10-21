interface EpisodeSearchPayload {
  episodeId: number;
  searchQuery?: string;
}

interface SeasonSearchPayload {
  seriesId: number;
  seasonNumber: number;
}

type InteractiveSearchPayload = EpisodeSearchPayload | SeasonSearchPayload;

export default InteractiveSearchPayload;
