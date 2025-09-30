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

export interface GrabbedHistoryData {
  indexer: string;
  nzbInfoUrl: string;
  releaseGroup: string;
  age: string;
  ageHours: string;
  ageMinutes: string;
  publishedDate: string;
  downloadClient: string;
  downloadClientName: string;
  size: string;
  downloadUrl: string;
  guid: string;
  tvdbId: string;
  tvRageId: string;
  protocol: string;
  customFormatScore?: string;
  seriesMatchType: string;
  releaseSource: string;
  indexerFlags: string;
  releaseType: string;
}

export interface DownloadFailedHistory {
  message: string;
  indexer?: string;
  source?: string;
}

export interface DownloadFolderImportedHistory {
  customFormatScore?: string;
  downloadClient: string;
  downloadClientName: string;
  droppedPath: string;
  importedPath: string;
  size: string;
}

export interface EpisodeFileDeletedHistory {
  customFormatScore?: string;
  reason: 'Manual' | 'MissingFromDisk' | 'Upgrade';
  size: string;
}

export interface EpisodeFileRenamedHistory {
  sourcePath: string;
  sourceRelativePath: string;
  path: string;
  relativePath: string;
}

export interface DownloadIgnoredHistory {
  message: string;
}

export type HistoryData =
  | GrabbedHistoryData
  | DownloadFailedHistory
  | DownloadFolderImportedHistory
  | EpisodeFileDeletedHistory
  | EpisodeFileRenamedHistory
  | DownloadIgnoredHistory;

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
  data: HistoryData;
  id: number;
}
