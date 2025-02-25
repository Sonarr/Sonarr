import React from 'react';
import getLanguageName from 'Utilities/String/getLanguageName';
import translate from 'Utilities/String/translate';
import { useEpisodeFile } from './EpisodeFileProvider';

function formatLanguages(languages: string[] | undefined) {
  if (!languages) {
    return null;
  }

  const splitLanguages = [...new Set(languages)].map((l) => {
    const simpleLanguage = l.split('_')[0];

    if (simpleLanguage === 'und') {
      return translate('Unknown');
    }

    return getLanguageName(simpleLanguage);
  });

  if (splitLanguages.length > 3) {
    return (
      <span title={splitLanguages.join(', ')}>
        {splitLanguages.slice(0, 2).join(', ')}, {splitLanguages.length - 2}{' '}
        more
      </span>
    );
  }

  return <span>{splitLanguages.join(', ')}</span>;
}

export type MediaInfoType =
  | 'audio'
  | 'audioLanguages'
  | 'subtitles'
  | 'video'
  | 'videoDynamicRangeType';

interface MediaInfoProps {
  episodeFileId?: number;
  type: MediaInfoType;
}

function MediaInfo({ episodeFileId, type }: MediaInfoProps) {
  const episodeFile = useEpisodeFile(episodeFileId);

  if (!episodeFile?.mediaInfo) {
    return null;
  }

  const {
    audioStreams = [],
    subtitleStreams = [],
    videoCodec,
    videoDynamicRangeType,
  } = episodeFile.mediaInfo;

  if (type === 'audio') {
    const [
      { channels: audioChannels, codec: audioCodec } = {
        channels: null,
        codec: null,
      },
    ] = audioStreams;

    return (
      <span>
        {audioCodec ? audioCodec : ''}

        {audioCodec && audioChannels ? ' - ' : ''}

        {audioChannels ? audioChannels.toFixed(1) : ''}
      </span>
    );
  }

  if (type === 'audioLanguages') {
    return formatLanguages(audioStreams.map(({ language }) => language));
  }

  if (type === 'subtitles') {
    return formatLanguages(subtitleStreams.map(({ language }) => language));
  }

  if (type === 'video') {
    return <span>{videoCodec}</span>;
  }

  if (type === 'videoDynamicRangeType') {
    return <span>{videoDynamicRangeType}</span>;
  }

  return null;
}

export default MediaInfo;
