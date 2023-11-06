import ModelBase from 'App/ModelBase';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import CustomFormat from 'typings/CustomFormat';

export type QueueTrackedDownloadStatus = 'ok' | 'warning' | 'error';

export type QueueTrackedDownloadState =
  | 'downloading'
  | 'importPending'
  | 'importing'
  | 'imported'
  | 'failedPending'
  | 'failed'
  | 'ignored';

export interface StatusMessage {
  title: string;
  messages: string[];
}

interface Queue extends ModelBase {
  languages: Language[];
  quality: QualityModel;
  customFormats: CustomFormat[];
  size: number;
  title: string;
  sizeleft: number;
  timeleft: string;
  estimatedCompletionTime: string;
  status: string;
  trackedDownloadStatus: QueueTrackedDownloadStatus;
  trackedDownloadState: QueueTrackedDownloadState;
  statusMessages: StatusMessage[];
  errorMessage: string;
  downloadId: string;
  protocol: string;
  downloadClient: string;
  outputPath: string;
  episodeHasFile: boolean;
  seriesId?: number;
  episodeId?: number;
  seasonNumber?: number;
}

export default Queue;
