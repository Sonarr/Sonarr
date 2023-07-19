import ModelBase from 'App/ModelBase';
import { AppSectionItemState } from 'App/State/AppSectionState';
import Episode from 'Episode/Episode';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import Series from 'Series/Series';
import CustomFormat from 'typings/CustomFormat';

export interface SeriesTitleInfo {
  title: string;
  titleWithoutYear: string;
  year: number;
  allTitles: string[];
}

export interface ParsedEpisodeInfo {
  releaseTitle: string;
  seriesTitle: string;
  seriesTitleInfo: SeriesTitleInfo;
  quality: QualityModel;
  seasonNumber: number;
  episodeNumbers: number[];
  absoluteEpisodeNumbers: number[];
  specialAbsoluteEpisodeNumbers: number[];
  languages: Language[];
  fullSeason: boolean;
  isPartialSeason: boolean;
  isMultiSeason: boolean;
  isSeasonExtra: boolean;
  special: boolean;
  releaseHash: string;
  seasonPart: number;
  releaseGroup?: string;
  releaseTokens: string;
  airDate?: string;
  isDaily: boolean;
  isAbsoluteNumbering: boolean;
  isPossibleSpecialEpisode: boolean;
  isPossibleSceneSeasonSpecial: boolean;
}

export interface ParseModel extends ModelBase {
  title: string;
  parsedEpisodeInfo: ParsedEpisodeInfo;
  series?: Series;
  episodes: Episode[];
  languages?: Language[];
  customFormats?: CustomFormat[];
  customFormatScore?: number;
}

type ParseAppState = AppSectionItemState<ParseModel>;

export default ParseAppState;
