import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch } from 'react-redux';
import { Tab, TabList, TabPanel, Tabs } from 'react-tabs';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import Episode from 'Episode/Episode';
import EpisodeDetailsTab from 'Episode/EpisodeDetailsTab';
import episodeEntities from 'Episode/episodeEntities';
import useEpisode, { EpisodeEntities } from 'Episode/useEpisode';
import Series from 'Series/Series';
import useSeries from 'Series/useSeries';
import { toggleEpisodeMonitored } from 'Store/Actions/episodeActions';
import {
  cancelFetchReleases,
  clearReleases,
} from 'Store/Actions/releaseActions';
import translate from 'Utilities/String/translate';
import EpisodeHistory from './History/EpisodeHistory';
import EpisodeSearch from './Search/EpisodeSearch';
import SeasonEpisodeNumber from './SeasonEpisodeNumber';
import EpisodeSummary from './Summary/EpisodeSummary';
import styles from './EpisodeDetailsModalContent.css';

const TABS: EpisodeDetailsTab[] = ['details', 'history', 'search'];

export interface EpisodeDetailsModalContentProps {
  episodeId: number;
  episodeEntity: EpisodeEntities;
  seriesId: number;
  episodeTitle: string;
  isSaving?: boolean;
  showOpenSeriesButton?: boolean;
  selectedTab?: EpisodeDetailsTab;
  startInteractiveSearch?: boolean;
  onTabChange(isSearch: boolean): void;
  onModalClose(): void;
}

function EpisodeDetailsModalContent(props: EpisodeDetailsModalContentProps) {
  const {
    episodeId,
    episodeEntity = episodeEntities.EPISODES,
    seriesId,
    episodeTitle,
    isSaving = false,
    showOpenSeriesButton = false,
    startInteractiveSearch = false,
    selectedTab = 'details',
    onTabChange,
    onModalClose,
  } = props;

  const dispatch = useDispatch();

  const [currentlySelectedTab, setCurrentlySelectedTab] = useState(selectedTab);

  const {
    title: seriesTitle,
    titleSlug,
    monitored: seriesMonitored,
    seriesType,
  } = useSeries(seriesId) as Series;

  const {
    episodeFileId,
    seasonNumber,
    episodeNumber,
    absoluteEpisodeNumber,
    airDate,
    monitored,
  } = useEpisode(episodeId, episodeEntity) as Episode;

  const handleTabSelect = useCallback(
    (selectedIndex: number) => {
      const tab = TABS[selectedIndex];
      onTabChange(tab === 'search');
      setCurrentlySelectedTab(tab);
    },
    [onTabChange]
  );

  const handleMonitorEpisodePress = useCallback(
    (monitored: boolean) => {
      dispatch(
        toggleEpisodeMonitored({
          episodeEntity,
          episodeId,
          monitored,
        })
      );
    },
    [episodeEntity, episodeId, dispatch]
  );

  useEffect(() => {
    return () => {
      // Clear pending releases here, so we can reshow the search
      // results even after switching tabs.
      dispatch(cancelFetchReleases());
      dispatch(clearReleases());
    };
  }, [dispatch]);

  const seriesLink = `/series/${titleSlug}`;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        <MonitorToggleButton
          monitored={monitored}
          size={18}
          isDisabled={!seriesMonitored}
          isSaving={isSaving}
          onPress={handleMonitorEpisodePress}
        />

        <span className={styles.seriesTitle}>{seriesTitle}</span>

        <span className={styles.separator}>-</span>

        <SeasonEpisodeNumber
          seasonNumber={seasonNumber}
          episodeNumber={episodeNumber}
          absoluteEpisodeNumber={absoluteEpisodeNumber}
          airDate={airDate}
          seriesType={seriesType}
        />

        <span className={styles.separator}>-</span>

        {episodeTitle}
      </ModalHeader>

      <ModalBody>
        <Tabs
          className={styles.tabs}
          selectedIndex={TABS.indexOf(currentlySelectedTab)}
          onSelect={handleTabSelect}
        >
          <TabList className={styles.tabList}>
            <Tab className={styles.tab} selectedClassName={styles.selectedTab}>
              {translate('Details')}
            </Tab>

            <Tab className={styles.tab} selectedClassName={styles.selectedTab}>
              {translate('History')}
            </Tab>

            <Tab className={styles.tab} selectedClassName={styles.selectedTab}>
              {translate('Search')}
            </Tab>
          </TabList>

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
            {/* Don't wrap in tabContent so we not have a top margin */}
            <EpisodeSearch
              episodeId={episodeId}
              startInteractiveSearch={startInteractiveSearch}
              onModalClose={onModalClose}
            />
          </TabPanel>
        </Tabs>
      </ModalBody>

      <ModalFooter>
        {showOpenSeriesButton && (
          <Button
            className={styles.openSeriesButton}
            to={seriesLink}
            onPress={onModalClose}
          >
            {translate('OpenSeries')}
          </Button>
        )}

        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default EpisodeDetailsModalContent;
