import ModelBase from 'App/ModelBase';

interface LogFile extends ModelBase {
  filename: string;
  lastWriteTime: string;
  contentsUrl: string;
  downloadUrl: string;
}

export default LogFile;
