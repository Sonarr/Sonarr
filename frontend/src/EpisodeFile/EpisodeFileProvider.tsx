import React, {
  createContext,
  PropsWithChildren,
  useContext,
  useMemo,
} from 'react';
import { EpisodeFile } from './EpisodeFile';
import useEpisodeFiles, { EpisodeFileFilter } from './useEpisodeFiles';

export const EpisodeFileContext = createContext<EpisodeFile[] | undefined>(
  undefined
);

export default function EpisodeFileProvider({
  children,
  ...filter
}: PropsWithChildren<EpisodeFileFilter>) {
  const { data } = useEpisodeFiles(filter);

  return (
    <EpisodeFileContext.Provider value={data}>
      {children}
    </EpisodeFileContext.Provider>
  );
}

export function useEpisodeFile(id: number | undefined) {
  const episodeFiles = useContext(EpisodeFileContext);

  return useMemo(() => {
    if (id === undefined) {
      return undefined;
    }

    return episodeFiles?.find((item) => item.id === id);
  }, [id, episodeFiles]);
}

export interface SeriesEpisodeFile {
  count: number;
  episodesWithFiles: number;
}
