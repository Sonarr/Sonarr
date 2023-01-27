import ModelBase from 'App/ModelBase';

export interface Image {
  coverType: string;
  url: string;
  remoteUrl: string;
}

export interface Language {
  id: number;
  name: string;
}

export interface Statistics {
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
  added: Date;
  alternateTitles: AlternateTitle[];
  cleanTitle: string;
  ended: boolean;
  firstAired: Date;
  genres: string[];
  images: Image[];
  imdbId: string;
  monitored: boolean;
  network: string;
  originalLanguage: Language;
  overview: string;
  path: string;
  previousAiring: Date;
  qualityProfileId: number;
  ratings: Ratings;
  rootFolderPath: string;
  runtime: number;
  seasonFolder: boolean;
  seasons: Season[];
  seriesType: string;
  sortTitle: string;
  statistics: Statistics;
  status: string;
  tags: number[];
  title: string;
  titleSlug: string;
  tvdbId: number;
  tvMazeId: number;
  tvRageId: number;
  useSceneNumbering: boolean;
  year: number;
}

export default Series;
