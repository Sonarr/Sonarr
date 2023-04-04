import ModelBase from 'App/ModelBase';
import AppSectionState, {
  AppSectionDeleteState,
} from 'App/State/AppSectionState';

export interface Tag extends ModelBase {
  label: string;
}

interface TagsAppState extends AppSectionState<Tag>, AppSectionDeleteState {}

export default TagsAppState;
