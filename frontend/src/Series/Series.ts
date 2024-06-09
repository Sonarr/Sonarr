import ModelBase from 'App/ModelBase';
import Language from 'Language/Language';

export type SeriesType = 'anime' | 'daily' | 'standard';

export interface Image {
  coverType: string;
  url: string;
  remoteUrl: string;
}

export interface Statistics {
  seasonCount: number;
  episodeCount: number;
  episodeFileCount: number;
  percentOfEpisodes: number;
  previousAiring?: Date;
  releaseGroups: string[];
  sizeOnDisk: number;
  totalEpisodeCount: number;
}

export interface Season {
  monitored: boolean;
  seasonNumber: number;
  statistics: Statistics;
  isSaving?: boolean;
}

export interface Ratings {
  votes: number;
  value: number;
}

export interface AlternateTitle {
  seasonNumber: number;
  title: string;
}

interface Series extends ModelBase {
  added: string;
  alternateTitles: AlternateTitle[];
  certification: string;
  cleanTitle: string;
  ended: boolean;
  firstAired: string;
  genres: string[];
  images: Image[];
  imdbId: string;
  monitored: boolean;
  network: string;
  originalLanguage: Language;
  overview: string;
  path: string;
  previousAiring?: string;
  nextAiring?: string;
  qualityProfileId: number;
  ratings: Ratings;
  rootFolderPath: string;
  runtime: number;
  seasonFolder: boolean;
  seasons: Season[];
  seriesType: SeriesType;
  sortTitle: string;
  statistics: Statistics;
  status: string;
  tags: number[];
  title: string;
  titleSlug: string;
  tvdbId: number;
  tvMazeId: number;
  tvRageId: number;
  tmdbId: number;
  useSceneNumbering: boolean;
  year: number;
  isSaving?: boolean;
}

export default Series;
