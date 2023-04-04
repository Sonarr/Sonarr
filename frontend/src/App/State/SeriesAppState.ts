import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSaveState,
} from 'App/State/AppSectionState';
import Column from 'Components/Table/Column';
import SortDirection from 'Helpers/Props/SortDirection';
import Series from 'Series/Series';
import { Filter, FilterBuilderProp } from './AppState';

export interface SeriesIndexAppState {
  sortKey: string;
  sortDirection: SortDirection;
  secondarySortKey: string;
  secondarySortDirection: SortDirection;
  view: string;

  posterOptions: {
    detailedProgressBar: boolean;
    size: string;
    showTitle: boolean;
    showMonitored: boolean;
    showQualityProfile: boolean;
    showSearchAction: boolean;
  };

  overviewOptions: {
    detailedProgressBar: boolean;
    size: string;
    showMonitored: boolean;
    showNetwork: boolean;
    showQualityProfile: boolean;
    showPreviousAiring: boolean;
    showAdded: boolean;
    showSeasonCount: boolean;
    showPath: boolean;
    showSizeOnDisk: boolean;
    showSearchAction: boolean;
  };

  tableOptions: {
    showBanners: boolean;
    showSearchAction: boolean;
  };

  selectedFilterKey: string;
  filterBuilderProps: FilterBuilderProp<Series>[];
  filters: Filter[];
  columns: Column[];
}

interface SeriesAppState
  extends AppSectionState<Series>,
    AppSectionDeleteState,
    AppSectionSaveState {
  itemMap: Record<number, number>;

  deleteOptions: {
    addImportListExclusion: boolean;
  };
}

export default SeriesAppState;
