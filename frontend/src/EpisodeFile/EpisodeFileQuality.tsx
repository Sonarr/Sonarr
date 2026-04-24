import React from 'react';
import Label from 'Components/Label';
import EpisodeQuality from 'Episode/EpisodeQuality';
import translate from 'Utilities/String/translate';
import { useEpisodeFile } from './EpisodeFileProvider';

interface EpisodeFileQualityProps {
  episodeFileId: number | undefined;
}

function EpisodeFileQuality({ episodeFileId }: EpisodeFileQualityProps) {
  const episodeFile = useEpisodeFile(episodeFileId);

  if (!episodeFile?.quality) {
    return <Label>{translate('Unknown')}</Label>;
  }

  return <EpisodeQuality quality={episodeFile.quality} />;
}

export default EpisodeFileQuality;
