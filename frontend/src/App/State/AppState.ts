import InteractiveImportAppState from 'App/State/InteractiveImportAppState';
import BlocklistAppState from './BlocklistAppState';
import CalendarAppState from './CalendarAppState';
import CommandAppState from './CommandAppState';
import EpisodeFilesAppState from './EpisodeFilesAppState';
import EpisodesAppState from './EpisodesAppState';
import HistoryAppState from './HistoryAppState';
import ParseAppState from './ParseAppState';
import QueueAppState from './QueueAppState';
import RootFolderAppState from './RootFolderAppState';
import SeriesAppState, { SeriesIndexAppState } from './SeriesAppState';
import SettingsAppState from './SettingsAppState';
import SystemAppState from './SystemAppState';
import TagsAppState from './TagsAppState';

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
  filers: PropertyFilter[];
}

export interface CustomFilter {
  id: number;
  type: string;
  label: string;
  filers: PropertyFilter[];
}

export interface AppSectionState {
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
  episodesSelection: EpisodesAppState;
  history: HistoryAppState;
  interactiveImport: InteractiveImportAppState;
  parse: ParseAppState;
  queue: QueueAppState;
  rootFolders: RootFolderAppState;
  series: SeriesAppState;
  seriesIndex: SeriesIndexAppState;
  settings: SettingsAppState;
  system: SystemAppState;
  tags: TagsAppState;
}

export default AppState;
