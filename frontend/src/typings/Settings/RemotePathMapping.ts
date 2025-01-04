import ModelBase from 'App/ModelBase';

interface RemotePathMapping extends ModelBase {
  host: string;
  localPath: string;
  remotePath: string;
}

export default RemotePathMapping;
