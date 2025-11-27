import { keepPreviousData } from '@tanstack/react-query';
import { useMemo } from 'react';
import useApiQuery from 'Helpers/Hooks/useApiQuery';

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

interface PathsResponse {
  parent: string | null;
  directories: Folder[];
  files: File[];
  paths: Path[];
}

const DEFAULT: PathsResponse = {
  parent: null,
  directories: [],
  files: [],
  paths: [],
};

const usePaths = ({
  path,
  allowFoldersWithoutTrailingSlashes = false,
  includeFiles = false,
}: {
  path: string;
  allowFoldersWithoutTrailingSlashes?: boolean;
  includeFiles?: boolean;
}) => {
  const { data: responseData, ...result } = useApiQuery<PathsResponse>({
    path: '/filesystem',
    queryParams: { path, allowFoldersWithoutTrailingSlashes, includeFiles },
    queryOptions: {
      enabled: path.trim().length > 0,
      placeholderData: keepPreviousData,
    },
  });

  const data = useMemo(() => {
    if (!responseData) {
      return DEFAULT;
    }

    const { directories, files, parent } = responseData;

    const filteredPaths = [...directories, ...files].filter((item) => {
      return item.path.toLowerCase().startsWith(path.toLowerCase());
    });

    return {
      directories,
      files,
      parent,
      paths: filteredPaths,
    };
  }, [path, responseData]);

  return {
    ...result,
    data,
  };
};

export default usePaths;
