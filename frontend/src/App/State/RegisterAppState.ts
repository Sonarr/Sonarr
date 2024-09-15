import ModelBase from 'App/ModelBase';
import AppSectionState, {
  AppSectionSaveState,
} from 'App/State/AppSectionState';

export interface Register extends ModelBase {
  username: string;
  password: string;
  passwordConfirmation: string;
}

export interface RegisterAppState
  extends AppSectionState<Register>,
    AppSectionSaveState {}

export default RegisterAppState;
