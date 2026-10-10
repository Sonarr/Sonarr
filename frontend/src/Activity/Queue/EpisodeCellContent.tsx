import React from 'react';
import Episode from 'Episode/Episode';
import SeasonEpisodeNumber from 'Episode/SeasonEpisodeNumber';
import Series from 'Series/Series';
import translate from 'Utilities/String/translate';

interface EpisodeCellContentProps {
  episodes: Episode[];
  isFullSeason: boolean;
  seasonNumbers: number[];
  series?: Series;
}

export default function EpisodeCellContent({
  episodes,
  isFullSeason,
  seasonNumbers,
  series,
}: EpisodeCellContentProps) {
  if (episodes.length === 0) {
    return '-';
  }

  if (isFullSeason && seasonNumbers.length > 1) {
    return translate('SeasonRangeToken', {
      firstSeasonNumber: seasonNumbers[0],
      lastSeasonNumber: seasonNumbers[seasonNumbers.length - 1],
    });
  }

  if (isFullSeason && seasonNumbers.length === 1) {
    return translate('SeasonNumberToken', { seasonNumber: seasonNumbers[0] });
  }

  if (episodes.length === 1) {
    const episode = episodes[0];

    return (
      <SeasonEpisodeNumber
        seasonNumber={episode.seasonNumber}
        episodeNumber={episode.episodeNumber}
        absoluteEpisodeNumber={episode.absoluteEpisodeNumber}
        seriesType={series?.seriesType}
        alternateTitles={series?.alternateTitles}
        sceneSeasonNumber={episode.sceneSeasonNumber}
        sceneEpisodeNumber={episode.sceneEpisodeNumber}
        sceneAbsoluteEpisodeNumber={episode.sceneAbsoluteEpisodeNumber}
        unverifiedSceneNumbering={episode.unverifiedSceneNumbering}
      />
    );
  }

  const firstEpisode = episodes[0];
  const lastEpisode = episodes[episodes.length - 1];

  return (
    <>
      <SeasonEpisodeNumber
        seasonNumber={firstEpisode.seasonNumber}
        episodeNumber={firstEpisode.episodeNumber}
        absoluteEpisodeNumber={firstEpisode.absoluteEpisodeNumber}
        seriesType={series?.seriesType}
        alternateTitles={series?.alternateTitles}
        sceneSeasonNumber={firstEpisode.sceneSeasonNumber}
        sceneEpisodeNumber={firstEpisode.sceneEpisodeNumber}
        sceneAbsoluteEpisodeNumber={firstEpisode.sceneAbsoluteEpisodeNumber}
        unverifiedSceneNumbering={firstEpisode.unverifiedSceneNumbering}
      />
      {' - '}
      <SeasonEpisodeNumber
        seasonNumber={lastEpisode.seasonNumber}
        episodeNumber={lastEpisode.episodeNumber}
        absoluteEpisodeNumber={lastEpisode.absoluteEpisodeNumber}
        seriesType={series?.seriesType}
        alternateTitles={series?.alternateTitles}
        sceneSeasonNumber={lastEpisode.sceneSeasonNumber}
        sceneEpisodeNumber={lastEpisode.sceneEpisodeNumber}
        sceneAbsoluteEpisodeNumber={lastEpisode.sceneAbsoluteEpisodeNumber}
        unverifiedSceneNumbering={lastEpisode.unverifiedSceneNumbering}
      />
    </>
  );
}
