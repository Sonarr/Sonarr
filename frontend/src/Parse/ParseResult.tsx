import React from 'react';
import { ParseModel } from 'App/State/ParseAppState';
import FieldSet from 'Components/FieldSet';
import EpisodeFormats from 'Episode/EpisodeFormats';
import SeriesTitleLink from 'Series/SeriesTitleLink';
import translate from 'Utilities/String/translate';
import ParseResultItem from './ParseResultItem';
import styles from './ParseResult.css';

interface ParseResultProps {
  item: ParseModel;
}

function ParseResult(props: ParseResultProps) {
  const { item } = props;
  const {
    customFormats,
    customFormatScore,
    episodes,
    languages,
    parsedEpisodeInfo,
    series,
  } = item;

  const {
    releaseTitle,
    seriesTitle,
    seriesTitleInfo,
    releaseGroup,
    releaseHash,
    seasonNumber,
    episodeNumbers,
    absoluteEpisodeNumbers,
    special,
    fullSeason,
    isMultiSeason,
    isPartialSeason,
    isDaily,
    airDate,
    quality,
  } = parsedEpisodeInfo;

  const finalLanguages = languages ?? parsedEpisodeInfo.languages;

  return (
    <div>
      <FieldSet legend={translate('Release')}>
        <ParseResultItem
          title={translate('ReleaseTitle')}
          data={releaseTitle}
        />

        <ParseResultItem title={translate('SeriesTitle')} data={seriesTitle} />

        <ParseResultItem
          title={translate('Year')}
          data={seriesTitleInfo.year > 0 ? seriesTitleInfo.year : '-'}
        />

        <ParseResultItem
          title={translate('AllTitles')}
          data={
            seriesTitleInfo.allTitles?.length > 0
              ? seriesTitleInfo.allTitles.join(', ')
              : '-'
          }
        />

        <ParseResultItem
          title={translate('ReleaseGroup')}
          data={releaseGroup ?? '-'}
        />

        <ParseResultItem
          title={translate('ReleaseHash')}
          data={releaseHash ? releaseHash : '-'}
        />
      </FieldSet>

      <FieldSet legend={translate('EpisodeInfo')}>
        <div className={styles.container}>
          <div className={styles.column}>
            <ParseResultItem
              title={translate('SeasonNumber')}
              data={
                seasonNumber === 0 && absoluteEpisodeNumbers.length
                  ? '-'
                  : seasonNumber
              }
            />

            <ParseResultItem
              title={translate('EpisodeNumbers')}
              data={episodeNumbers.join(', ') || '-'}
            />

            <ParseResultItem
              title={translate('AbsoluteEpisodeNumbers')}
              data={
                absoluteEpisodeNumbers.length
                  ? absoluteEpisodeNumbers.join(', ')
                  : '-'
              }
            />

            <ParseResultItem
              title={translate('Daily')}
              data={isDaily ? 'True' : 'False'}
            />

            <ParseResultItem
              title={translate('AirDate')}
              data={airDate ?? '-'}
            />
          </div>

          <div className={styles.column}>
            <ParseResultItem
              title={translate('Special')}
              data={special ? 'True' : 'False'}
            />

            <ParseResultItem
              title={translate('FullSeason')}
              data={fullSeason ? 'True' : 'False'}
            />

            <ParseResultItem
              title={translate('MultiSeason')}
              data={isMultiSeason ? 'True' : 'False'}
            />

            <ParseResultItem
              title={translate('PartialSeason')}
              data={isPartialSeason ? 'True' : 'False'}
            />
          </div>
        </div>
      </FieldSet>

      <FieldSet legend={translate('Quality')}>
        <div className={styles.container}>
          <div className={styles.column}>
            <ParseResultItem
              title={translate('Quality')}
              data={quality.quality.name}
            />
            <ParseResultItem
              title={translate('Proper')}
              data={
                quality.revision.version > 1 && !quality.revision.isRepack
                  ? 'True'
                  : '-'
              }
            />

            <ParseResultItem
              title={translate('Repack')}
              data={quality.revision.isRepack ? 'True' : '-'}
            />
          </div>

          <div className={styles.column}>
            <ParseResultItem
              title={translate('Version')}
              data={
                quality.revision.version > 1 ? quality.revision.version : '-'
              }
            />

            <ParseResultItem
              title={translate('Real')}
              data={quality.revision.real ? 'True' : '-'}
            />
          </div>
        </div>
      </FieldSet>

      <FieldSet legend={translate('Languages')}>
        <ParseResultItem
          title={translate('Languages')}
          data={finalLanguages.map((l) => l.name).join(', ')}
        />
      </FieldSet>

      <FieldSet legend={translate('Details')}>
        <ParseResultItem
          title={translate('MatchedToSeries')}
          data={
            series ? (
              <SeriesTitleLink
                titleSlug={series.titleSlug}
                title={series.title}
              />
            ) : (
              '-'
            )
          }
        />

        <ParseResultItem
          title={translate('MatchedToSeason')}
          data={episodes.length ? episodes[0].seasonNumber : '-'}
        />

        <ParseResultItem
          title={translate('MatchedToEpisodes')}
          data={
            episodes.length ? (
              <div>
                {episodes.map((e) => {
                  return (
                    <div key={e.id}>
                      {e.episodeNumber}
                      {series?.seriesType === 'anime' && e.absoluteEpisodeNumber
                        ? ` (${e.absoluteEpisodeNumber})`
                        : ''}{' '}
                      {` - ${e.title}`}
                    </div>
                  );
                })}
              </div>
            ) : (
              '-'
            )
          }
        />

        <ParseResultItem
          title={translate('CustomFormats')}
          data={<EpisodeFormats formats={customFormats} />}
        />

        <ParseResultItem
          title={translate('CustomFormatScore')}
          data={customFormatScore}
        />
      </FieldSet>
    </div>
  );
}

export default ParseResult;
