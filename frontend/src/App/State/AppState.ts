import ModelBase from 'App/ModelBase';
import { FilterBuilderTypes } from 'Helpers/Props/filterBuilderTypes';
import { DateFilterValue, FilterType } from 'Helpers/Props/filterTypes';
import { Error } from './AppSectionState';
import BlocklistAppState from './BlocklistAppState';
import CalendarAppState from './CalendarAppState';
import CaptchaAppState from './CaptchaAppState';
import CommandAppState from './CommandAppState';
import CustomFiltersAppState from './CustomFiltersAppState';
import EpisodeFilesAppState from './EpisodeFilesAppState';
import EpisodesAppState from './EpisodesAppState';
import HistoryAppState, { SeriesHistoryAppState } from './HistoryAppState';
import ImportSeriesAppState from './ImportSeriesAppState';
import InteractiveImportAppState from './InteractiveImportAppState';
import MessagesAppState from './MessagesAppState';
import OAuthAppState from './OAuthAppState';
import OrganizePreviewAppState from './OrganizePreviewAppState';
import ParseAppState from './ParseAppState';
import PathsAppState from './PathsAppState';
import ProviderOptionsAppState from './ProviderOptionsAppState';
import QueueAppState from './QueueAppState';
import ReleasesAppState from './ReleasesAppState';
import RootFolderAppState from './RootFolderAppState';
import SeriesAppState, { SeriesIndexAppState } from './SeriesAppState';
import SettingsAppState from './SettingsAppState';
import SystemAppState from './SystemAppState';
import TagsAppState from './TagsAppState';
import WantedAppState from './WantedAppState';

export interface FilterBuilderPropOption {
  id: string;
  name: string;
}

export interface FilterBuilderProp<T> {
  name: string;
  label: string | (() => string);
  type: FilterBuilderTypes;
  valueType?: string;
  optionsSelector?: (items: T[]) => FilterBuilderPropOption[];
}

export interface PropertyFilter {
  key: string;
  value: string | string[] | number[] | boolean[] | DateFilterValue;
  type: FilterType;
}

export interface Filter {
  key: string;
  label: string | (() => string);
  type: string;
  filters: PropertyFilter[];
}

export interface CustomFilter extends ModelBase {
  type: string;
  label: string;
  filters: PropertyFilter[];
}

export interface AppSectionState {
  isUpdated: boolean;
  isConnected: boolean;
  isDisconnected: boolean;
  isReconnecting: boolean;
  isRestarting: boolean;
  isSidebarVisible: boolean;
  version: string;
  prevVersion?: string;
  dimensions: {
    isSmallScreen: boolean;
    isLargeScreen: boolean;
    width: number;
    height: number;
  };
  translations: {
    error?: Error;
    isPopulated: boolean;
  };
  messages: MessagesAppState;
}

interface AppState {
  app: AppSectionState;
  blocklist: BlocklistAppState;
  calendar: CalendarAppState;
  captcha: CaptchaAppState;
  commands: CommandAppState;
  customFilters: CustomFiltersAppState;
  episodeFiles: EpisodeFilesAppState;
  episodeHistory: HistoryAppState;
  episodes: EpisodesAppState;
  episodesSelection: EpisodesAppState;
  history: HistoryAppState;
  importSeries: ImportSeriesAppState;
  interactiveImport: InteractiveImportAppState;
  oAuth: OAuthAppState;
  organizePreview: OrganizePreviewAppState;
  parse: ParseAppState;
  paths: PathsAppState;
  providerOptions: ProviderOptionsAppState;
  queue: QueueAppState;
  releases: ReleasesAppState;
  rootFolders: RootFolderAppState;
  series: SeriesAppState;
  seriesHistory: SeriesHistoryAppState;
  seriesIndex: SeriesIndexAppState;
  settings: SettingsAppState;
  system: SystemAppState;
  tags: TagsAppState;
  wanted: WantedAppState;
}

export default AppState;
