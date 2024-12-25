import ModelBase from 'App/ModelBase';

export type BackupType = 'manual' | 'scheduled' | 'update';

interface Backup extends ModelBase {
  name: string;
  path: string;
  type: BackupType;
  size: number;
  time: string;
}

export default Backup;
