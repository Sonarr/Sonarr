import ModelBase from 'App/ModelBase';

export interface UnmappedFolder {
  name: string;
  path: string;
  relativePath: string;
}

interface RootFolder extends ModelBase {
  id: number;
  path: string;
  accessible: boolean;
  freeSpace?: number;
  unmappedFolders: UnmappedFolder[];
}

export default RootFolder;
