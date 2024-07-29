interface BasePath {
  name: string;
  path: string;
  size: number;
  lastModified: string;
}

interface File extends BasePath {
  type: 'file';
}

interface Folder extends BasePath {
  type: 'folder';
}

export type PathType = 'file' | 'folder' | 'drive' | 'computer' | 'parent';
export type Path = File | Folder;

interface PathsAppState {
  currentPath: string;
  isFetching: boolean;
  isPopulated: boolean;
  error: Error;
  directories: Folder[];
  files: File[];
  parent: string | null;
}

export default PathsAppState;
