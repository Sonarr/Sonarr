import AppSectionState, {
  AppSectionDeleteState,
  AppSectionItemState,
  AppSectionListState,
  AppSectionSaveState,
  AppSectionSchemaState,
} from 'App/State/AppSectionState';
import CustomFormat from 'typings/CustomFormat';
import CustomFormatSpecification from 'typings/CustomFormatSpecification';
import DelayProfile from 'typings/DelayProfile';
import DownloadClient from 'typings/DownloadClient';
import ImportList from 'typings/ImportList';
import ImportListOptionsSettings from 'typings/ImportListOptionsSettings';
import DownloadClientOptions from 'typings/Settings/DownloadClientOptions';

type Presets<T> = T & {
  presets: T[];
};

export interface DelayProfileAppState
  extends AppSectionListState<DelayProfile>,
    AppSectionDeleteState,
    AppSectionSaveState {}

export interface DownloadClientAppState
  extends AppSectionState<DownloadClient>,
    AppSectionDeleteState,
    AppSectionSaveState,
    AppSectionSchemaState<Presets<DownloadClient>> {
  isTestingAll: boolean;
}

export interface DownloadClientOptionsAppState
  extends AppSectionItemState<DownloadClientOptions>,
    AppSectionSaveState {}

export interface ImportListAppState
  extends AppSectionState<ImportList>,
    AppSectionDeleteState,
    AppSectionSaveState,
    AppSectionSchemaState<Presets<ImportList>> {
  isTestingAll: boolean;
}

export interface CustomFormatAppState
  extends AppSectionState<CustomFormat>,
    AppSectionDeleteState,
    AppSectionSaveState {}

export interface CustomFormatSpecificationAppState
  extends AppSectionState<CustomFormatSpecification>,
    AppSectionDeleteState,
    AppSectionSaveState,
    AppSectionSchemaState<Presets<CustomFormatSpecification>> {}

export interface ImportListOptionsSettingsAppState
  extends AppSectionItemState<ImportListOptionsSettings>,
    AppSectionSaveState {}

interface SettingsAppState {
  customFormats: CustomFormatAppState;
  customFormatSpecifications: CustomFormatSpecificationAppState;
  delayProfiles: DelayProfileAppState;
  downloadClients: DownloadClientAppState;
  downloadClientOptions: DownloadClientOptionsAppState;
  importListOptions: ImportListOptionsSettingsAppState;
  importLists: ImportListAppState;
}

export default SettingsAppState;
