import ModelBase from 'App/ModelBase';
import ReleaseType from 'InteractiveImport/ReleaseType';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import CustomFormat from 'typings/CustomFormat';
import MediaInfo from 'typings/MediaInfo';

export interface EpisodeFile extends ModelBase {
  seriesId: number;
  seasonNumber: number;
  relativePath: string;
  path: string;
  size: number;
  dateAdded: string;
  sceneName: string;
  releaseGroup: string;
  languages: Language[];
  quality: QualityModel;
  customFormats: CustomFormat[];
  customFormatScore: number;
  indexerFlags: number;
  releaseType: ReleaseType;
  mediaInfo: MediaInfo;
  qualityCutoffNotMet: boolean;
}
