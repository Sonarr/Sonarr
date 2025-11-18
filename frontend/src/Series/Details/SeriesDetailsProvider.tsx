import React, { PropsWithChildren } from 'react';
import QueueDetailsProvider from 'Activity/Queue/Details/QueueDetailsProvider';
import { EpisodeFileContext } from 'EpisodeFile/EpisodeFileProvider';
import useEpisodeFiles from 'EpisodeFile/useEpisodeFiles';

function SeriesDetailsProvider({
  seriesId,
  children,
}: PropsWithChildren<{ seriesId: number }>) {
  const { data } = useEpisodeFiles({ seriesId });

  return (
    <QueueDetailsProvider seriesId={seriesId}>
      <EpisodeFileContext.Provider value={data}>
        {children}
      </EpisodeFileContext.Provider>
    </QueueDetailsProvider>
  );
}

export default SeriesDetailsProvider;
