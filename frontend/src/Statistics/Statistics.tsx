import React, { useCallback, useMemo } from 'react';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSpacer from 'Components/Page/Toolbar/PageToolbarSpacer';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import { align, kinds } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import BarChart, { BarChartItem } from './Charts/BarChart';
import DoughnutChart, { DoughnutChartItem } from './Charts/DoughnutChart';
import StatisticsFilterModal from './StatisticsFilterModal';
import {
  setStatisticsOption,
  useStatisticsOption,
} from './statisticsOptionsStore';
import StatisticsSummary, { SummaryItem } from './StatisticsSummary';
import useChartColors from './useChartColors';
import useStatistics, { FILTERS } from './useStatistics';
import styles from './Statistics.css';

function Statistics() {
  const selectedFilterKey = useStatisticsOption('selectedFilterKey');
  const customFilters = useCustomFiltersList('statistics');
  const colors = useChartColors();

  const { data, isLoading, error } = useStatistics();

  const handleFilterSelect = useCallback((key: string | number) => {
    setStatisticsOption('selectedFilterKey', key);
  }, []);

  const summaryItems = useMemo<SummaryItem[]>(() => {
    if (!data) {
      return [];
    }

    const downloadedPercent = data.totalEpisodeCount
      ? Math.round((data.downloadedEpisodeCount / data.totalEpisodeCount) * 100)
      : 0;

    const wantedEpisodeCount =
      data.downloadedEpisodeCount + data.missingEpisodeCount;
    const completedPercent = wantedEpisodeCount
      ? Math.round((data.downloadedEpisodeCount / wantedEpisodeCount) * 100)
      : 0;

    const averageSizePerEpisode = data.totalEpisodeCount
      ? data.sizeOnDisk / data.totalEpisodeCount
      : 0;

    return [
      {
        label: translate('Series'),
        value: data.seriesCount.toLocaleString(),
        secondary: `${data.monitoredSeriesCount.toLocaleString()} ${translate(
          'Monitored'
        )} · ${data.completedSeriesCount.toLocaleString()} ${translate(
          'Completed'
        )}`,
      },
      {
        label: translate('Seasons'),
        value: data.seasonCount.toLocaleString(),
        secondary: `${data.completedSeasonCount.toLocaleString()} ${translate(
          'Completed'
        )}`,
      },
      {
        label: translate('TotalEpisodes'),
        value: data.totalEpisodeCount.toLocaleString(),
        secondary: `${data.monitoredEpisodeCount.toLocaleString()} ${translate(
          'Monitored'
        )}`,
      },
      {
        label: translate('EpisodeFiles'),
        value: data.episodeFileCount.toLocaleString(),
        secondary: `${downloadedPercent}% ${translate('Downloaded')}`,
      },
      {
        label: translate('MissingEpisodes'),
        value: data.missingEpisodeCount.toLocaleString(),
        secondary: `${completedPercent}% ${translate('Completed')}`,
      },
      {
        label: translate('SizeOnDisk'),
        value: formatBytes(data.sizeOnDisk),
        secondary: `${formatBytes(averageSizePerEpisode)} ${translate(
          'AverageSizePerEpisode'
        )}`,
      },
    ];
  }, [data]);

  const statusItems = useMemo<DoughnutChartItem[]>(() => {
    if (!data) {
      return [];
    }

    return [
      {
        label: translate('Continuing'),
        value: data.continuingSeriesCount,
        color: colors.successColor,
      },
      {
        label: translate('Ended'),
        value: data.endedSeriesCount,
        color: colors.bar,
      },
      {
        label: translate('Upcoming'),
        value: data.upcomingSeriesCount,
        color: colors.warningColor,
      },
      {
        label: translate('Deleted'),
        value: data.deletedSeriesCount,
        color: colors.dangerColor,
      },
    ];
  }, [colors, data]);

  const typeItems = useMemo<DoughnutChartItem[]>(() => {
    if (!data) {
      return [];
    }

    return [
      {
        label: translate('Standard'),
        value: data.standardSeriesCount,
        color: colors.bar,
      },
      {
        label: translate('Daily'),
        value: data.dailySeriesCount,
        color: colors.warningColor,
      },
      {
        label: translate('Anime'),
        value: data.animeSeriesCount,
        color: colors.palette[4],
      },
    ];
  }, [colors, data]);

  const episodeItems = useMemo<DoughnutChartItem[]>(() => {
    if (!data) {
      return [];
    }

    const otherCount = Math.max(
      0,
      data.totalEpisodeCount -
        data.downloadedEpisodeCount -
        data.missingEpisodeCount -
        data.unairedEpisodeCount
    );

    return [
      {
        label: translate('Downloaded'),
        value: data.downloadedEpisodeCount,
        color: colors.successColor,
      },
      {
        label: translate('Missing'),
        value: data.missingEpisodeCount,
        color: colors.dangerColor,
      },
      {
        label: translate('Unaired'),
        value: data.unairedEpisodeCount,
        color: colors.palette[6],
      },
      {
        label: translate('Unmonitored'),
        value: otherCount,
        color: colors.grayColor,
      },
    ];
  }, [colors, data]);

  const qualityProfileItems = useMemo<BarChartItem[]>(() => {
    if (!data) {
      return [];
    }

    return data.qualityProfileStatistics.map((profile) => {
      return {
        label: profile.name,
        value: profile.seriesCount,
        tooltipLines: [
          `${translate(
            'EpisodeFiles'
          )}: ${profile.episodeFileCount.toLocaleString()}`,
          `${translate('SizeOnDisk')}: ${formatBytes(profile.sizeOnDisk)}`,
        ],
      };
    });
  }, [data]);

  const qualityItems = useMemo<BarChartItem[]>(() => {
    if (!data) {
      return [];
    }

    return data.qualityStatistics.map((quality) => {
      return {
        label: quality.quality.name,
        value: quality.episodeFileCount,
        tooltipLines: [
          `${translate('SizeOnDisk')}: ${formatBytes(quality.sizeOnDisk)}`,
        ],
      };
    });
  }, [data]);

  const tagItems = useMemo<BarChartItem[]>(() => {
    if (!data) {
      return [];
    }

    return data.tagStatistics.map((tag) => {
      return {
        label: tag.label,
        value: tag.seriesCount,
        tooltipLines: [
          `${translate(
            'EpisodeFiles'
          )}: ${tag.episodeFileCount.toLocaleString()}`,
          `${translate('SizeOnDisk')}: ${formatBytes(tag.sizeOnDisk)}`,
        ],
      };
    });
  }, [data]);

  return (
    <PageContent title={translate('Statistics')}>
      <PageToolbar>
        <PageToolbarSpacer />

        <ToolbarItem id="filter" pinned={true}>
          <FilterMenu
            alignMenu={align.RIGHT}
            selectedFilterKey={selectedFilterKey}
            filters={FILTERS}
            customFilters={customFilters}
            filterModalConnectorComponent={StatisticsFilterModal}
            onFilterSelect={handleFilterSelect}
          />
        </ToolbarItem>
      </PageToolbar>

      <PageContentBody>
        <PageHeading
          scope={translate('Media')}
          title={translate('Statistics')}
        />

        {isLoading ? <LoadingIndicator /> : null}

        {!isLoading && error ? (
          <Alert kind={kinds.DANGER}>{translate('StatisticsLoadError')}</Alert>
        ) : null}

        {!isLoading && !error && data ? (
          <div className={styles.chartsContainer}>
            <StatisticsSummary items={summaryItems} />

            <div className={styles.charts}>
              <DoughnutChart
                title={translate('SeriesByStatus')}
                items={statusItems}
              />

              <DoughnutChart
                title={translate('SeriesType')}
                items={typeItems}
              />

              <DoughnutChart
                title={translate('Episodes')}
                items={episodeItems}
              />
            </div>

            <div className={styles.charts}>
              <BarChart
                title={translate('QualityProfiles')}
                items={qualityProfileItems}
              />

              <BarChart
                title={translate('EpisodeQualities')}
                items={qualityItems}
              />

              {tagItems.length ? (
                <BarChart title={translate('SeriesTags')} items={tagItems} />
              ) : null}
            </div>
          </div>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default Statistics;
