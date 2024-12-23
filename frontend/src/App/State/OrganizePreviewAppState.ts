import ModelBase from 'App/ModelBase';
import AppSectionState from 'App/State/AppSectionState';

export interface OrganizePreviewModel extends ModelBase {
  seriesId: number;
  seasonNumber: number;
  episodeNumbers: number[];
  episodeFileId: number;
  existingPath: string;
  newPath: string;
}

type OrganizePreviewAppState = AppSectionState<OrganizePreviewModel>;

export default OrganizePreviewAppState;
