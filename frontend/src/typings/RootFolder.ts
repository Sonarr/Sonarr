import ModelBase from 'App/ModelBase';

interface RootFolder extends ModelBase {
  id: number;
  path: string;
  accessible: boolean;
  freeSpace?: number;
  unmappedFolders: object[];
}

export default RootFolder;
