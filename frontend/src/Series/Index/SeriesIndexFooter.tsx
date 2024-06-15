import classNames from 'classnames';
import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import { ColorImpairedConsumer } from 'App/ColorImpairedContext';
import SeriesAppState from 'App/State/SeriesAppState';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import { Statistics } from 'Series/Series';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createDeepEqualSelector from 'Store/Selectors/createDeepEqualSelector';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './SeriesIndexFooter.css';

interface SeriesStatistics {
  monitored: boolean;
  status: string;
  statistics: Statistics;
}

function createAllSeriesStatisticsSelector() {
  return createSelector(createAllSeriesSelector(), (series) => {
    return series.map((s) => {
      const { monitored, status, statistics } = s;

      return {
        monitored,
        status,
        statistics,
      };
    });
  });
}

function createUnoptimizedSeriesSelector() {
  return createSelector(
    createClientSideCollectionSelector('series', 'seriesIndex'),
    (series: SeriesAppState) => {
      return series.items.map((s) => {
        const { monitored, status, statistics } = s;

        return {
          monitored,
          status,
          statistics,
        };
      });
    }
  );
}

function createSeriesSelector() {
  return createDeepEqualSelector(
    createUnoptimizedSeriesSelector(),
    (series) => series
  );
}

function calculateSeriesStatistics(series: SeriesStatistics[]) {
  return series.reduce(
    (acc, s) => {
      const { statistics = {} as Statistics } = s;
      const {
        episodeCount = 0,
        episodeFileCount = 0,
        sizeOnDisk = 0,
      } = statistics;

      acc.episodes += episodeCount;
      acc.episodeFiles += episodeFileCount;
      acc.totalFileSize += sizeOnDisk;

      if (s.status === 'ended') {
        acc.ended++;
      } else {
        acc.continuing++;
      }

      if (s.monitored) {
        acc.monitored++;
      }

      return acc;
    },
    {
      episodes: 0,
      episodeFiles: 0,
      totalFileSize: 0,
      ended: 0,
      continuing: 0,
      monitored: 0,
    }
  );
}

export default function SeriesIndexFooter() {
  const allSeries = useSelector(createAllSeriesStatisticsSelector());
  const series = useSelector(createSeriesSelector());

  const allCount = allSeries.length;
  const count = series.length;

  const {
    episodes: allEpisodes,
    episodeFiles: allEpisodeFiles,
    ended: allEnded,
    continuing: allContinuing,
    monitored: allMonitored,
    totalFileSize: allTotalFileSize,
  } = useMemo(() => calculateSeriesStatistics(allSeries), [allSeries]);

  const {
    episodes,
    episodeFiles,
    ended,
    continuing,
    monitored,
    totalFileSize,
  } = useMemo(() => calculateSeriesStatistics(series), [series]);

  const unmonitored = count - monitored;
  const allUnmonitored = allCount - allMonitored;

  return (
    <ColorImpairedConsumer>
      {(enableColorImpairedMode) => {
        return (
          <div className={styles.footer}>
            <div>
              <div className={styles.legendItem}>
                <div
                  className={classNames(
                    styles.continuing,
                    enableColorImpairedMode && 'colorImpaired'
                  )}
                />
                <div>{translate('SeriesIndexFooterContinuing')}</div>
              </div>

              <div className={styles.legendItem}>
                <div
                  className={classNames(
                    styles.ended,
                    enableColorImpairedMode && 'colorImpaired'
                  )}
                />
                <div>{translate('SeriesIndexFooterEnded')}</div>
              </div>

              <div className={styles.legendItem}>
                <div
                  className={classNames(
                    styles.missingMonitored,
                    enableColorImpairedMode && 'colorImpaired'
                  )}
                />
                <div>{translate('SeriesIndexFooterMissingMonitored')}</div>
              </div>

              <div className={styles.legendItem}>
                <div
                  className={classNames(
                    styles.missingUnmonitored,
                    enableColorImpairedMode && 'colorImpaired'
                  )}
                />
                <div>{translate('SeriesIndexFooterMissingUnmonitored')}</div>
              </div>

              <div className={styles.legendItem}>
                <div
                  className={classNames(
                    styles.downloading,
                    enableColorImpairedMode && 'colorImpaired'
                  )}
                />
                <div>{translate('SeriesIndexFooterDownloading')}</div>
              </div>
            </div>

            <div className={styles.statistics}>
              <DescriptionList>
                <DescriptionListItem
                  title={translate('Series')}
                  data={count === allCount ? count : `${count}/${allCount}`}
                />

                <DescriptionListItem
                  title={translate('Ended')}
                  data={count === allCount ? ended : `${ended}/${allEnded}`}
                />

                <DescriptionListItem
                  title={translate('Continuing')}
                  data={
                    count === allCount
                      ? continuing
                      : `${continuing}/${allContinuing}`
                  }
                />
              </DescriptionList>

              <DescriptionList>
                <DescriptionListItem
                  title={translate('Monitored')}
                  data={
                    count === allCount
                      ? monitored
                      : `${monitored}/${allMonitored}`
                  }
                />

                <DescriptionListItem
                  title={translate('Unmonitored')}
                  data={
                    count === allCount
                      ? unmonitored
                      : `${unmonitored}/${allUnmonitored}`
                  }
                />
              </DescriptionList>

              <DescriptionList>
                <DescriptionListItem
                  title={translate('Episodes')}
                  data={
                    count === allCount ? episodes : `${episodes}/${allEpisodes}`
                  }
                />

                <DescriptionListItem
                  title={translate('Files')}
                  data={
                    count === allCount
                      ? episodeFiles
                      : `${episodeFiles}/${allEpisodeFiles}`
                  }
                />
              </DescriptionList>

              <DescriptionList>
                <DescriptionListItem
                  title={translate('TotalFileSize')}
                  data={
                    count === allCount
                      ? formatBytes(totalFileSize)
                      : `${formatBytes(totalFileSize)}/${formatBytes(
                          allTotalFileSize
                        )}`
                  }
                />
              </DescriptionList>
            </div>
          </div>
        );
      }}
    </ColorImpairedConsumer>
  );
}
