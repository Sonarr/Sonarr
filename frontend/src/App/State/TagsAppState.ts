import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSaveState,
} from 'App/State/AppSectionState';
import { TagDetail } from 'Tags/useTagDetails';
import { Tag } from 'Tags/useTags';

export interface TagDetailAppState
  extends AppSectionState<TagDetail>,
    AppSectionDeleteState,
    AppSectionSaveState {}

interface TagsAppState extends AppSectionState<Tag>, AppSectionDeleteState {
  details: TagDetailAppState;
}

export default TagsAppState;
