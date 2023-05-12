import InteractiveImportAppState from 'App/State/InteractiveImportAppState';
import CalendarAppState from './CalendarAppState';
import EpisodeFilesAppState from './EpisodeFilesAppState';
import EpisodesAppState from './EpisodesAppState';
import QueueAppState from './QueueAppState';
import SeriesAppState, { SeriesIndexAppState } from './SeriesAppState';
import SettingsAppState from './SettingsAppState';
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

interface AppState {
  calendar: CalendarAppState;
  episodesSelection: EpisodesAppState;
  episodeFiles: EpisodeFilesAppState;
  interactiveImport: InteractiveImportAppState;
  seriesIndex: SeriesIndexAppState;
  settings: SettingsAppState;
  series: SeriesAppState;
  tags: TagsAppState;
  queue: QueueAppState;
}

export default AppState;
