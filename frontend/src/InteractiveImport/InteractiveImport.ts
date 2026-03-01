import ModelBase from 'App/ModelBase';
import Episode from 'Episode/Episode';
import ReleaseType from 'InteractiveImport/ReleaseType';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import Series from 'Series/Series';
import CustomFormat from 'typings/CustomFormat';
import Rejection from 'typings/Rejection';

export interface InteractiveImportCommandOptions {
  path: string;
  folderName: string;
  seriesId: number;
  episodeIds: number[];
  releaseGroup?: string;
  quality: QualityModel;
  languages: Language[];
  indexerFlags: number;
  releaseType: ReleaseType;
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
  customFormats: CustomFormat[];
  indexerFlags: number;
  releaseType: ReleaseType;
  rejections: Rejection[];
  episodeFileId?: number;
  downloadId?: string;
}

export default InteractiveImport;
