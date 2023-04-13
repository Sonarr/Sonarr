import ModelBase from 'App/ModelBase';
import Episode from 'Episode/Episode';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import Series from 'Series/Series';
import Rejection from 'typings/Rejection';

export interface InteractiveImportCommandOptions {
  path: string;
  folderName: string;
  seriesId: number;
  episodeIds: number[];
  releaseGroup?: string;
  quality: QualityModel;
  languages: Language[];
  downloadId?: string;
  episodeFileId?: number;
}

interface InteractiveImport extends ModelBase {
  path: string;
  relativePath: string;
  folderName: string;
  name: string;
  size: number;
  releaseGroup: string;
  quality: QualityModel;
  languages: Language[];
  series?: Series;
  seasonNumber: number;
  episodes: Episode[];
  qualityWeight: number;
  customFormats: object[];
  rejections: Rejection[];
  episodeFileId?: number;
}

export default InteractiveImport;
