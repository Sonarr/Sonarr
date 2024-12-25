import Backup from 'typings/Backup';
import AppSectionState, { Error } from './AppSectionState';

interface BackupAppState extends AppSectionState<Backup> {
  isRestoring: boolean;
  restoreError?: Error;
}

export default BackupAppState;
