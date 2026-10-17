import { useQueryClient } from '@tanstack/react-query';
import React, { useCallback, useMemo, useRef, useState } from 'react';
import { Tab, TabList, TabPanel, Tabs } from 'react-tabs';
import { EpisodeQueueDetailsProvider } from 'Activity/Queue/Details/QueueDetailsProvider';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import SpinnerIcon from 'Components/SpinnerIcon';
import Episode from 'Episode/Episode';
import EpisodeDetailsTab from 'Episode/EpisodeDetailsTab';
import episodeEntities from 'Episode/episodeEntities';
import EpisodeStatusLabel from 'Episode/EpisodeStatusLabel';
import useEpisode, {
  EpisodeEntity,
  getQueryKey,
  useToggleEpisodesMonitored,
} from 'Episode/useEpisode';
import { icons, kinds, sizes } from 'Helpers/Props';
import InteractiveSearchFilters from 'InteractiveSearch/InteractiveSearchFilters';
import {
  hasFetchedReleases,
  useClearReleasesOnUnmount,
} from 'InteractiveSearch/useReleases';
import Series from 'Series/Series';
import { useSingleSeries } from 'Series/useSeries';
import translate from 'Utilities/String/translate';
import EpisodeHistory from './History/EpisodeHistory';
import EpisodeSearch from './Search/EpisodeSearch';
import SeasonEpisodeNumber from './SeasonEpisodeNumber';
import EpisodeSummary from './Summary/EpisodeSummary';
import styles from './EpisodeDetailsModalContent.module.css';

const TABS: EpisodeDetailsTab[] = ['details', 'history', 'search'];

export interface EpisodeDetailsModalContentProps {
  episodeId: number;
  episodeEntity: EpisodeEntity;
  seriesId: number;
  episodeTitle: string;
  selectedTab?: EpisodeDetailsTab;
  startInteractiveSearch?: boolean;
  onTabChange(isSearch: boolean): void;
  onModalClose(): void;
}

function EpisodeDetailsModalContent({
  episodeId,
  episodeEntity = episodeEntities.EPISODES,
  seriesId,
  episodeTitle,
  startInteractiveSearch = false,
  selectedTab = 'details',
  onTabChange,
  onModalClose,
}: EpisodeDetailsModalContentProps) {
  const [currentlySelectedTab, setCurrentlySelectedTab] = useState(selectedTab);
  const modalBodyRef = useRef<HTMLDivElement>(null);
  const [filter, setFilter] = useState('');

  const queryClient = useQueryClient();

  const [isInteractiveSearchOpen, setIsInteractiveSearchOpen] = useState(
    () =>
      startInteractiveSearch || hasFetchedReleases(queryClient, { episodeId })
  );

  const {
    title: seriesTitle,
    titleSlug,
    monitored: seriesMonitored,
    seriesType,
    runtime,
  } = useSingleSeries(seriesId) as Series;

  const episode = useEpisode(episodeId, episodeEntity) as Episode;

  const {
    episodeFileId,
    seasonNumber,
    episodeNumber,
    absoluteEpisodeNumber,
    airDate,
    monitored,
  } = episode;

  const { toggleEpisodesMonitored, isToggling } = useToggleEpisodesMonitored(
    getQueryKey(episodeEntity)!
  );

  useClearReleasesOnUnmount({ episodeId });

  const handleTabSelect = useCallback(
    (selectedIndex: number) => {
      const tab = TABS[selectedIndex];
      onTabChange(tab === 'search');
      setCurrentlySelectedTab(tab);
    },
    [onTabChange]
  );

  const handleMonitorEpisodePress = useCallback(() => {
    toggleEpisodesMonitored({
      episodeIds: [episodeId],
      monitored: !monitored,
    });
  }, [episodeId, monitored, toggleEpisodesMonitored]);

  const canToggleMonitored = seriesMonitored || monitored;

  let monitorTitle = translate('ToggleMonitoredSeriesUnmonitored');

  if (canToggleMonitored) {
    monitorTitle = monitored
      ? translate('ToggleMonitoredToUnmonitored')
      : translate('ToggleUnmonitoredToMonitored');
  }

  const queueEpisodeIds = useMemo(() => [episodeId], [episodeId]);

  const searchPayload = useMemo(() => ({ episodeId }), [episodeId]);

  const handleInteractiveSearchPress = useCallback(() => {
    setIsInteractiveSearchOpen(true);
  }, []);

  const seriesLink = `/series/${titleSlug}`;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader className={styles.header}>
        <div className={styles.scope}>
          <Link
            className={styles.scopeSeries}
            to={seriesLink}
            onPress={onModalClose}
          >
            {seriesTitle}
          </Link>

          <span className={styles.scopeSeparator}>/</span>

          <span className={styles.scopeEpisode}>
            <SeasonEpisodeNumber
              seasonNumber={seasonNumber}
              episodeNumber={episodeNumber}
              absoluteEpisodeNumber={absoluteEpisodeNumber}
              airDate={airDate}
              seriesType={seriesType}
            />
          </span>
        </div>

        <div className={styles.episodeTitle}>{episodeTitle}</div>

        <div className={styles.labels}>
          <Link
            className={styles.monitoredToggle}
            title={monitorTitle}
            aria-label={monitorTitle}
            isDisabled={!canToggleMonitored || isToggling}
            onPress={handleMonitorEpisodePress}
          >
            <Label
              kind={monitored ? kinds.DEFAULT : kinds.DISABLED}
              size={sizes.MEDIUM}
              dot={false}
              interactive={canToggleMonitored && !isToggling}
            >
              <SpinnerIcon
                name={icons.MONITORED}
                filled={monitored}
                size={12}
                isSpinning={isToggling}
              />
              {monitored ? translate('Monitored') : translate('Unmonitored')}
            </Label>
          </Link>

          <EpisodeQueueDetailsProvider episodeIds={queueEpisodeIds}>
            <EpisodeStatusLabel
              episode={episode}
              hasFile={!!episodeFileId}
              seriesMonitored={seriesMonitored}
              seriesRuntime={runtime}
            />
          </EpisodeQueueDetailsProvider>
        </div>
      </ModalHeader>

      <ModalBody ref={modalBodyRef}>
        <Tabs
          className={styles.tabs}
          selectedIndex={TABS.indexOf(currentlySelectedTab)}
          onSelect={handleTabSelect}
        >
          <div className={styles.tabBar}>
            <TabList className={styles.tabList}>
              <Tab
                className={styles.tab}
                selectedClassName={styles.selectedTab}
              >
                {translate('Details')}
              </Tab>

              <Tab
                className={styles.tab}
                selectedClassName={styles.selectedTab}
              >
                {translate('History')}
              </Tab>

              <Tab
                className={styles.tab}
                selectedClassName={styles.selectedTab}
              >
                {translate('Search')}
              </Tab>
            </TabList>

            {currentlySelectedTab === 'search' && isInteractiveSearchOpen ? (
              <InteractiveSearchFilters
                className={styles.tabFilters}
                searchPayload={searchPayload}
                filter={filter}
                onFilterChange={setFilter}
              />
            ) : null}
          </div>

          <TabPanel>
            <div className={styles.tabContent}>
              <EpisodeSummary
                episodeId={episodeId}
                episodeEntity={episodeEntity}
                episodeFileId={episodeFileId}
                seriesId={seriesId}
              />
            </div>
          </TabPanel>

          <TabPanel>
            <div className={styles.tabContent}>
              <EpisodeHistory episodeId={episodeId} />
            </div>
          </TabPanel>

          <TabPanel>
            <div
              className={
                isInteractiveSearchOpen ? styles.tabContent : undefined
              }
            >
              <EpisodeSearch
                episodeId={episodeId}
                isInteractiveSearchOpen={isInteractiveSearchOpen}
                filter={filter}
                scrollerRef={modalBodyRef}
                onInteractiveSearchPress={handleInteractiveSearchPress}
                onModalClose={onModalClose}
              />
            </div>
          </TabPanel>
        </Tabs>
      </ModalBody>
    </ModalContent>
  );
}

export default EpisodeDetailsModalContent;
