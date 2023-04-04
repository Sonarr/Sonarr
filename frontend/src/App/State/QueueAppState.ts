import ModelBase from 'App/ModelBase';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import CustomFormat from 'typings/CustomFormat';
import AppSectionState, { AppSectionItemState, Error } from './AppSectionState';

export interface StatusMessage {
  title: string;
  messages: string[];
}

export interface Queue extends ModelBase {
  languages: Language[];
  quality: QualityModel;
  customFormats: CustomFormat[];
  size: number;
  title: string;
  sizeleft: number;
  timeleft: string;
  estimatedCompletionTime: string;
  status: string;
  trackedDownloadStatus: string;
  trackedDownloadState: string;
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

export interface QueueDetailsAppState extends AppSectionState<Queue> {
  params: unknown;
}

export interface QueuePagedAppState extends AppSectionState<Queue> {
  isGrabbing: boolean;
  grabError: Error;
  isRemoving: boolean;
  removeError: Error;
}

interface QueueAppState {
  status: AppSectionItemState<Queue>;
  details: QueueDetailsAppState;
  paged: QueuePagedAppState;
}

export default QueueAppState;
