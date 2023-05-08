import React from 'react';
import { ParseModel } from 'App/State/ParseAppState';
import FieldSet from 'Components/FieldSet';
import EpisodeFormats from 'Episode/EpisodeFormats';
import SeriesTitleLink from 'Series/SeriesTitleLink';
import translate from 'Utilities/String/translate';
import ParseResultItem from './ParseResultItem';

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
          title={translate('Release Title')}
          data={releaseTitle}
        />

        <ParseResultItem title={translate('Series Title')} data={seriesTitle} />

        <ParseResultItem
          title={translate('Year')}
          data={seriesTitleInfo.year > 0 ? seriesTitleInfo.year : '-'}
        />

        <ParseResultItem
          title={translate('All Titles')}
          data={
            seriesTitleInfo.allTitles?.length > 0
              ? seriesTitleInfo.allTitles.join(', ')
              : '-'
          }
        />

        <ParseResultItem
          title={translate('Release Group')}
          data={releaseGroup ?? '-'}
        />

        <ParseResultItem
          title={translate('Release Hash')}
          data={releaseHash ? releaseHash : '-'}
        />
      </FieldSet>

      {/* 
      
      Year
      Secondary titles
      special episode
      
      */}

      <FieldSet legend={translate('Episode Info')}>
        <ParseResultItem
          title={translate('Season Number')}
          data={
            seasonNumber === 0 && absoluteEpisodeNumbers.length
              ? '-'
              : seasonNumber
          }
        />

        <ParseResultItem
          title={translate('Episode Number(s)')}
          data={episodeNumbers.join(', ') || '-'}
        />

        <ParseResultItem
          title={translate('Absolute Episode Number(s)')}
          data={
            absoluteEpisodeNumbers.length
              ? absoluteEpisodeNumbers.join(', ')
              : '-'
          }
        />

        <ParseResultItem
          title={translate('Special')}
          data={special ? 'True' : 'False'}
        />

        <ParseResultItem
          title={translate('Full Season')}
          data={fullSeason ? 'True' : 'False'}
        />

        <ParseResultItem
          title={translate('Multi-Season')}
          data={isMultiSeason ? 'True' : 'False'}
        />

        <ParseResultItem
          title={translate('Partial Season')}
          data={isPartialSeason ? 'True' : 'False'}
        />

        <ParseResultItem
          title={translate('Daily')}
          data={isDaily ? 'True' : 'False'}
        />

        <ParseResultItem title={translate('Air Date')} data={airDate ?? '-'} />
      </FieldSet>

      <FieldSet legend={translate('Quality')}>
        <ParseResultItem
          title={translate('Quality')}
          data={quality.quality.name}
        />

        <ParseResultItem
          title={translate('Version')}
          data={quality.revision.version > 1 ? quality.revision.version : '-'}
        />

        <ParseResultItem
          title={translate('Real')}
          data={quality.revision.real ? 'True' : '-'}
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
      </FieldSet>

      <FieldSet legend={translate('Languages')}>
        <ParseResultItem
          title={translate('Languages')}
          data={finalLanguages.map((l) => l.name).join(', ')}
        />
      </FieldSet>

      <FieldSet legend={translate('Details')}>
        <ParseResultItem
          title={translate('Matched to Series')}
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
          title={translate('Matched to Season')}
          data={episodes.length ? episodes[0].seasonNumber : '-'}
        />

        <ParseResultItem
          title={translate('Matched to Episodes')}
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
          title={translate('Custom Formats')}
          data={<EpisodeFormats formats={customFormats} />}
        />

        <ParseResultItem
          title={translate('Custom Format Score')}
          data={customFormatScore}
        />
      </FieldSet>
    </div>
  );
}

export default ParseResult;
