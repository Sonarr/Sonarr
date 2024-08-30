import BlocklistAppState from './BlocklistAppState';
import CalendarAppState from './CalendarAppState';
import CommandAppState from './CommandAppState';
import EpisodeFilesAppState from './EpisodeFilesAppState';
import EpisodesAppState from './EpisodesAppState';
import HistoryAppState from './HistoryAppState';
import InteractiveImportAppState from './InteractiveImportAppState';
import ParseAppState from './ParseAppState';
import PathsAppState from './PathsAppState';
import QueueAppState from './QueueAppState';
import ReleasesAppState from './ReleasesAppState';
import RootFolderAppState from './RootFolderAppState';
import SeriesAppState, { SeriesIndexAppState } from './SeriesAppState';
import SettingsAppState from './SettingsAppState';
import SystemAppState from './SystemAppState';
import TagsAppState from './TagsAppState';
import WantedAppState from './WantedAppState';

interface FilterBuilderPropOption {
  id: string;
  name: string;
}

export interface FilterBuilderProp<T> {
  name: string;
  label: string;
  type: string;
  valueType?: string;
  optionsSelector?: (items: T[]) => FilterBuilderPropOption[];
}

export interface PropertyFilter {
  key: string;
  value: boolean | string | number | string[] | number[];
  type: string;
}

export interface Filter {
  key: string;
  label: string;
  filters: PropertyFilter[];
}

export interface CustomFilter {
  id: number;
  type: string;
  label: string;
  filters: PropertyFilter[];
}

export interface AppSectionState {
  isConnected: boolean;
  isReconnecting: boolean;
  version: string;
  prevVersion?: string;
  dimensions: {
    isSmallScreen: boolean;
    width: number;
    height: number;
  };
}

interface AppState {
  app: AppSectionState;
  blocklist: BlocklistAppState;
  calendar: CalendarAppState;
  commands: CommandAppState;
  episodeFiles: EpisodeFilesAppState;
  episodes: EpisodesAppState;
  episodesSelection: EpisodesAppState;
  history: HistoryAppState;
  interactiveImport: InteractiveImportAppState;
  parse: ParseAppState;
  paths: PathsAppState;
  queue: QueueAppState;
  releases: ReleasesAppState;
  rootFolders: RootFolderAppState;
  series: SeriesAppState;
  seriesIndex: SeriesIndexAppState;
  settings: SettingsAppState;
  system: SystemAppState;
  tags: TagsAppState;
  wanted: WantedAppState;
}

export default AppState;
