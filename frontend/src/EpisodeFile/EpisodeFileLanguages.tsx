import React from 'react';
import EpisodeLanguages from 'Episode/EpisodeLanguages';
import useEpisodeFile from './useEpisodeFile';

interface EpisodeFileLanguagesProps {
  episodeFileId: number | undefined;
}

function EpisodeFileLanguages({ episodeFileId }: EpisodeFileLanguagesProps) {
  const episodeFile = useEpisodeFile(episodeFileId);

  return <EpisodeLanguages languages={episodeFile?.languages ?? []} />;
}

export default EpisodeFileLanguages;
