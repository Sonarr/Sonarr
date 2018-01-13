import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Tab, Tabs, TabList, TabPanel } from 'react-tabs';
import episodeEntities from 'Episode/episodeEntities';
import Button from 'Components/Link/Button';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import EpisodeSummaryConnector from './Summary/EpisodeSummaryConnector';
import EpisodeHistoryConnector from './History/EpisodeHistoryConnector';
import EpisodeSearchConnector from './Search/EpisodeSearchConnector';
import SeasonEpisodeNumber from './SeasonEpisodeNumber';
import styles from './EpisodeDetailsModalContent.css';

const tabs = [
  'details',
  'history',
  'search'
];

class EpisodeDetailsModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      selectedTab: props.selectedTab
    };
  }

  //
  // Listeners

  onTabSelect = (index, lastIndex) => {
    this.setState({ selectedTab: tabs[index] });
  }

  //
  // Render

  render() {
    const {
      episodeId,
      episodeEntity,
      episodeFileId,
      seriesId,
      seriesTitle,
      titleSlug,
      seriesMonitored,
      seriesType,
      seasonNumber,
      episodeNumber,
      absoluteEpisodeNumber,
      episodeTitle,
      airDate,
      monitored,
      isSaving,
      showOpenSeriesButton,
      startInteractiveSearch,
      onMonitorEpisodePress,
      onModalClose
    } = this.props;

    const seriesLink = `/series/${titleSlug}`;

    return (
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          <MonitorToggleButton
            className={styles.toggleButton}
            id={episodeId}
            monitored={monitored}
            size={18}
            isDisabled={!seriesMonitored}
            isSaving={isSaving}
            onPress={onMonitorEpisodePress}
          />

          <span className={styles.seriesTitle}>
            {seriesTitle}
          </span>

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
            selectedIndex={tabs.indexOf(this.state.selectedTab)}
            onSelect={this.onTabSelect}
          >
            <TabList
              className={styles.tabList}
            >
              <Tab
                className={styles.tab}
                selectedClassName={styles.selectedTab}
              >
                Details
              </Tab>

              <Tab
                className={styles.tab}
                selectedClassName={styles.selectedTab}
              >
                History
              </Tab>

              <Tab
                className={styles.tab}
                selectedClassName={styles.selectedTab}
              >
                Search
              </Tab>
            </TabList>

            <TabPanel>
              <div className={styles.tabContent}>
                <EpisodeSummaryConnector
                  episodeId={episodeId}
                  episodeEntity={episodeEntity}
                  episodeFileId={episodeFileId}
                  seriesId={seriesId}
                />
              </div>
            </TabPanel>

            <TabPanel>
              <div className={styles.tabContent}>
                <EpisodeHistoryConnector
                  episodeId={episodeId}
                />
              </div>
            </TabPanel>

            <TabPanel>
              {/* Don't wrap in tabContent so we not have a top margin */}
              <EpisodeSearchConnector
                episodeId={episodeId}
                startInteractiveSearch={startInteractiveSearch}
                onModalClose={onModalClose}
              />
            </TabPanel>
          </Tabs>
        </ModalBody>

        <ModalFooter>
          {
            showOpenSeriesButton &&
              <Button
                className={styles.openSeriesButton}
                to={seriesLink}
                onPress={onModalClose}
              >
                Open Series
              </Button>
          }

          <Button
            onPress={onModalClose}
          >
            Close
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

EpisodeDetailsModalContent.propTypes = {
  episodeId: PropTypes.number.isRequired,
  episodeEntity: PropTypes.string.isRequired,
  episodeFileId: PropTypes.number,
  seriesId: PropTypes.number.isRequired,
  seriesTitle: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  seriesMonitored: PropTypes.bool.isRequired,
  seriesType: PropTypes.string.isRequired,
  seasonNumber: PropTypes.number.isRequired,
  episodeNumber: PropTypes.number.isRequired,
  absoluteEpisodeNumber: PropTypes.number,
  airDate: PropTypes.string.isRequired,
  episodeTitle: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool,
  showOpenSeriesButton: PropTypes.bool,
  selectedTab: PropTypes.string.isRequired,
  startInteractiveSearch: PropTypes.bool.isRequired,
  onMonitorEpisodePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

EpisodeDetailsModalContent.defaultProps = {
  selectedTab: 'details',
  episodeEntity: episodeEntities.EPISODES,
  startInteractiveSearch: false
};

export default EpisodeDetailsModalContent;
