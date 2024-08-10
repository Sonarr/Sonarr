import ModelBase from 'App/ModelBase';
import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSaveState,
} from 'App/State/AppSectionState';

export interface User extends ModelBase {
  username: string;
}

export interface UserDetail extends ModelBase {
  username: string;
}

export interface UserDetailAppState
  extends AppSectionState<UserDetail>,
    AppSectionDeleteState,
    AppSectionSaveState {}

interface UsersAppState extends AppSectionState<User>, AppSectionDeleteState {
  details: UserDetailAppState;
}

export default UsersAppState;
