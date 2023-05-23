import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import CustomFormat from './CustomFormat';

export type HistoryEventType =
  | 'grabbed'
  | 'seriesFolderImported'
  | 'downloadFolderImported'
  | 'downloadFailed'
  | 'episodeFileDeleted'
  | 'episodeFileRenamed'
  | 'downloadIgnored';

export default interface History {
  episodeId: number;
  seriesId: number;
  sourceTitle: string;
  languages: Language[];
  quality: QualityModel;
  customFormats: CustomFormat[];
  customFormatScore: number;
  qualityCutoffNotMet: boolean;
  date: string;
  downloadId: string;
  eventType: HistoryEventType;
  data: unknown;
  id: number;
}
