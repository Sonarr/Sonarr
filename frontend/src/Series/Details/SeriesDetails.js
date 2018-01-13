import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Measure from 'react-measure';
import TextTruncate from 'react-text-truncate';
import formatBytes from 'Utilities/Number/formatBytes';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import { align, icons, kinds, sizes, tooltipPositions } from 'Helpers/Props';
import fonts from 'Styles/Variables/fonts';
import HeartRating from 'Components/HeartRating';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import Label from 'Components/Label';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import EpisodeFileEditorModal from 'EpisodeFile/Editor/EpisodeFileEditorModal';
import OrganizePreviewModalConnector from 'Organize/OrganizePreviewModalConnector';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import SeriesPoster from 'Series/SeriesPoster';
import EditSeriesModalConnector from 'Series/Edit/EditSeriesModalConnector';
import DeleteSeriesModal from 'Series/Delete/DeleteSeriesModal';
import SeriesHistoryModal from 'Series/History/SeriesHistoryModal';
import SeriesAlternateTitles from './SeriesAlternateTitles';
import SeriesDetailsSeasonConnector from './SeriesDetailsSeasonConnector';
import SeriesTagsConnector from './SeriesTagsConnector';
import SeriesDetailsLinks from './SeriesDetailsLinks';
import styles from './SeriesDetails.css';
import InteractiveImportModal from '../../InteractiveImport/InteractiveImportModal';

const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

function getFanartUrl(images) {
  const fanartImage = _.find(images, { coverType: 'fanart' });
  if (fanartImage) {
    // Remove protocol
    return fanartImage.url.replace(/^https?:/, '');
  }
}

function getExpandedState(newState) {
  return {
    allExpanded: newState.allSelected,
    allCollapsed: newState.allUnselected,
    expandedState: newState.selectedState
  };
}

class SeriesDetails extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isOrganizeModalOpen: false,
      isManageEpisodesOpen: false,
      isEditSeriesModalOpen: false,
      isDeleteSeriesModalOpen: false,
      isSeriesHistoryModalOpen: false,
      isInteractiveImportModalOpen: false,
      allExpanded: false,
      allCollapsed: false,
      expandedState: {},
      overviewHeight: 0
    };
  }

  //
  // Listeners

  onOrganizePress = () => {
    this.setState({ isOrganizeModalOpen: true });
  }

  onOrganizeModalClose = () => {
    this.setState({ isOrganizeModalOpen: false });
  }

  onManageEpisodesPress = () => {
    this.setState({ isManageEpisodesOpen: true });
  }

  onManageEpisodesModalClose = () => {
    this.setState({ isManageEpisodesOpen: false });
  }

  onInteractiveImportPress = () => {
    this.setState({ isInteractiveImportModalOpen: true });
  }

  onInteractiveImportModalClose = () => {
    this.setState({ isInteractiveImportModalOpen: false });
  }

  onEditSeriesPress = () => {
    this.setState({ isEditSeriesModalOpen: true });
  }

  onEditSeriesModalClose = () => {
    this.setState({ isEditSeriesModalOpen: false });
  }

  onDeleteSeriesPress = () => {
    this.setState({
      isEditSeriesModalOpen: false,
      isDeleteSeriesModalOpen: true
    });
  }

  onDeleteSeriesModalClose = () => {
    this.setState({ isDeleteSeriesModalOpen: false });
  }

  onSeriesHistoryPress = () => {
    this.setState({ isSeriesHistoryModalOpen: true });
  }

  onSeriesHistoryModalClose = () => {
    this.setState({ isSeriesHistoryModalOpen: false });
  }

  onExpandAllPress = () => {
    const {
      allExpanded,
      expandedState
    } = this.state;

    this.setState(getExpandedState(selectAll(expandedState, !allExpanded)));
  }

  onExpandPress = (seasonNumber, isExpanded) => {
    this.setState((state) => {
      const convertedState = {
        allSelected: state.allExpanded,
        allUnselected: state.allCollapsed,
        selectedState: state.expandedState
      };

      const newState = toggleSelected(convertedState, [], seasonNumber, isExpanded, false);

      return getExpandedState(newState);
    });
  }

  onMeasure = ({ height }) => {
    this.setState({ overviewHeight: height });
  }

  //
  // Render

  render() {
    const {
      id,
      tvdbId,
      tvMazeId,
      imdbId,
      title,
      runtime,
      ratings,
      path,
      statistics,
      qualityProfileId,
      monitored,
      status,
      network,
      overview,
      images,
      seasons,
      alternateTitles,
      tags,
      isSaving,
      isRefreshing,
      isSearching,
      isFetching,
      isPopulated,
      episodesError,
      episodeFilesError,
      hasMonitoredEpisodes,
      previousSeries,
      nextSeries,
      onMonitorTogglePress,
      onRefreshPress,
      onSearchPress
    } = this.props;

    const {
      episodeFileCount,
      sizeOnDisk
    } = statistics;

    const {
      isOrganizeModalOpen,
      isManageEpisodesOpen,
      isEditSeriesModalOpen,
      isDeleteSeriesModalOpen,
      isSeriesHistoryModalOpen,
      isInteractiveImportModalOpen,
      allExpanded,
      allCollapsed,
      expandedState,
      overviewHeight
    } = this.state;

    const continuing = status === 'continuing';

    let episodeFilesCountMessage = 'No episode files';

    if (episodeFileCount === 1) {
      episodeFilesCountMessage = '1 episode file';
    } else if (episodeFileCount > 1) {
      episodeFilesCountMessage = `${episodeFileCount} episode files`;
    }

    let expandIcon = icons.EXPAND_INDETERMINATE;

    if (allExpanded) {
      expandIcon = icons.COLLAPSE;
    } else if (allCollapsed) {
      expandIcon = icons.EXPAND;
    }

    return (
      <PageContent title={title}>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="Refresh & Scan"
              iconName={icons.REFRESH}
              spinningName={icons.REFRESH}
              title="Refresh information and scan disk"
              isSpinning={isRefreshing}
              onPress={onRefreshPress}
            />

            <PageToolbarButton
              label="Search Monitored"
              iconName={icons.SEARCH}
              isDisabled={!monitored || !hasMonitoredEpisodes}
              isSpinning={isSearching}
              title={hasMonitoredEpisodes ? undefined : 'No monitored episodes in this series'}
              onPress={onSearchPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label="Preview Rename"
              iconName={icons.ORGANIZE}
              onPress={this.onOrganizePress}
            />

            <PageToolbarButton
              label="Manage Episodes"
              iconName={icons.EPISODE_FILE}
              onPress={this.onManageEpisodesPress}
            />

            <PageToolbarButton
              label="History"
              iconName={icons.HISTORY}
              onPress={this.onSeriesHistoryPress}
            />

            <PageToolbarButton
              label="Manual Import"
              iconName={icons.INTERACTIVE}
              onPress={this.onInteractiveImportPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label="Edit"
              iconName={icons.EDIT}
              onPress={this.onEditSeriesPress}
            />

            <PageToolbarButton
              label="Delete"
              iconName={icons.DELETE}
              onPress={this.onDeleteSeriesPress}
            />
          </PageToolbarSection>

          <PageToolbarSection alignContent={align.RIGHT}>
            <PageToolbarButton
              label={allExpanded ? 'Collapse All' : 'Expand All'}
              iconName={expandIcon}
              onPress={this.onExpandAllPress}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBodyConnector innerClassName={styles.innerContentBody}>
          <div className={styles.header}>
            <div
              className={styles.backdrop}
              style={{
                backgroundImage: `url(${getFanartUrl(images)})`
              }}
            >
              <div className={styles.backdropOverlay} />
            </div>

            <div className={styles.headerContent}>
              <SeriesPoster
                className={styles.poster}
                images={images}
                size={500}
                lazy={false}
              />

              <div className={styles.info}>
                <div className={styles.titleRow}>
                  <div className={styles.titleContainer}>
                    <div className={styles.toggleMonitoredContainer}>
                      <MonitorToggleButton
                        className={styles.monitorToggleButton}
                        monitored={monitored}
                        isSaving={isSaving}
                        size={40}
                        onPress={onMonitorTogglePress}
                      />
                    </div>

                    <div className={styles.title}>
                      {title}
                    </div>

                    {
                      !!alternateTitles.length &&
                        <div className={styles.alternateTitlesIconContainer}>
                          <Popover
                            anchor={
                              <Icon
                                name={icons.ALTERNATE_TITLES}
                                size={20}
                              />
                            }
                            title="Alternate Titles"
                            body={<SeriesAlternateTitles alternateTitles={alternateTitles} />}
                            position={tooltipPositions.BOTTOM}
                          />
                        </div>
                    }
                  </div>

                  <div className={styles.seriesNavigationButtons}>
                    <IconButton
                      className={styles.seriesNavigationButton}
                      name={icons.ARROW_LEFT}
                      size={30}
                      title={`Go to ${previousSeries.title}`}
                      to={`/series/${previousSeries.titleSlug}`}
                    />

                    <IconButton
                      className={styles.seriesNavigationButton}
                      name={icons.ARROW_RIGHT}
                      size={30}
                      title={`Go to ${nextSeries.title}`}
                      to={`/series/${nextSeries.titleSlug}`}
                    />
                  </div>
                </div>

                <div className={styles.details}>
                  <div>
                    {
                      !!runtime &&
                        <span className={styles.runtime}>
                          {runtime} Minutes
                        </span>
                    }

                    <HeartRating
                      rating={ratings.value}
                      iconSize={20}
                    />
                  </div>
                </div>

                <div className={styles.detailsLabels}>
                  <Label
                    className={styles.detailsLabel}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.FOLDER}
                      size={17}
                    />

                    <span className={styles.path}>
                      {path}
                    </span>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    title={episodeFilesCountMessage}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.DRIVE}
                      size={17}
                    />

                    <span className={styles.sizeOnDisk}>
                      {
                        formatBytes(sizeOnDisk)
                      }
                    </span>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    title="Quality Profile"
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.PROFILE}
                      size={17}
                    />

                    <span className={styles.qualityProfileName}>
                      {
                        <QualityProfileNameConnector
                          qualityProfileId={qualityProfileId}
                        />
                      }
                    </span>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={monitored ? icons.MONITORED : icons.UNMONITORED}
                      size={17}
                    />

                    <span className={styles.qualityProfileName}>
                      {monitored ? 'Monitored' : 'Unmonitored'}
                    </span>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    title={continuing ? 'More episodes/another season is expected' : 'No additional episodes or or another season is expected'}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={continuing ? icons.SERIES_CONTINUING : icons.SERIES_ENDED}
                      size={17}
                    />

                    <span className={styles.qualityProfileName}>
                      {continuing ? 'Continuing' : 'Ended'}
                    </span>
                  </Label>

                  {
                    !!network &&
                      <Label
                        className={styles.detailsLabel}
                        title="Network"
                        size={sizes.LARGE}
                      >
                        <Icon
                          name={icons.NETWORK}
                          size={17}
                        />

                        <span className={styles.qualityProfileName}>
                          {network}
                        </span>
                      </Label>
                  }

                  <Tooltip
                    anchor={
                      <Label
                        className={styles.detailsLabel}
                        size={sizes.LARGE}
                      >
                        <Icon
                          name={icons.EXTERNAL_LINK}
                          size={17}
                        />

                        <span className={styles.links}>
                          Links
                        </span>
                      </Label>
                    }
                    tooltip={
                      <SeriesDetailsLinks
                        tvdbId={tvdbId}
                        tvMazeId={tvMazeId}
                        imdbId={imdbId}
                      />
                    }
                    kind={kinds.INVERSE}
                    position={tooltipPositions.BOTTOM}
                  />

                  {
                    !!tags.length &&
                      <Tooltip
                        anchor={
                          <Label
                            className={styles.detailsLabel}
                            size={sizes.LARGE}
                          >
                            <Icon
                              name={icons.TAGS}
                              size={17}
                            />

                            <span className={styles.tags}>
                              Tags
                            </span>
                          </Label>
                        }
                        tooltip={<SeriesTagsConnector seriesId={id} />}
                        kind={kinds.INVERSE}
                        position={tooltipPositions.BOTTOM}
                      />

                  }
                </div>

                <Measure onMeasure={this.onMeasure}>
                  <div className={styles.overview}>
                    <TextTruncate
                      line={Math.floor(overviewHeight / (defaultFontSize * lineHeight))}
                      text={overview}
                    />
                  </div>
                </Measure>
              </div>
            </div>
          </div>

          <div className={styles.contentContainer}>
            {
              !isPopulated && !episodesError && !episodeFilesError &&
                <LoadingIndicator />
            }

            {
              !isFetching && episodesError &&
                <div>Loading episodes failed</div>
            }

            {
              !isFetching && episodeFilesError &&
                <div>Loading episode files failed</div>
            }

            {
              isPopulated && !!seasons.length &&
                <div>
                  {
                    seasons.slice(0).reverse().map((season) => {
                      return (
                        <SeriesDetailsSeasonConnector
                          key={season.seasonNumber}
                          seriesId={id}
                          {...season}
                          isExpanded={expandedState[season.seasonNumber]}
                          onExpandPress={this.onExpandPress}
                        />
                      );
                    })
                  }
                </div>
            }

            {
              isPopulated && !seasons.length &&
                <div>
                  No episode information is available.
                </div>
            }

          </div>

          <OrganizePreviewModalConnector
            isOpen={isOrganizeModalOpen}
            seriesId={id}
            onModalClose={this.onOrganizeModalClose}
          />

          <EpisodeFileEditorModal
            isOpen={isManageEpisodesOpen}
            seriesId={id}
            onModalClose={this.onManageEpisodesModalClose}
          />

          <SeriesHistoryModal
            isOpen={isSeriesHistoryModalOpen}
            seriesId={id}
            onModalClose={this.onSeriesHistoryModalClose}
          />

          <EditSeriesModalConnector
            isOpen={isEditSeriesModalOpen}
            seriesId={id}
            onModalClose={this.onEditSeriesModalClose}
            onDeleteSeriesPress={this.onDeleteSeriesPress}
          />

          <DeleteSeriesModal
            isOpen={isDeleteSeriesModalOpen}
            seriesId={id}
            onModalClose={this.onDeleteSeriesModalClose}
          />

          <InteractiveImportModal
            isOpen={isInteractiveImportModalOpen}
            folder={path}
            showFilterExistingFiles={true}
            onModalClose={this.onInteractiveImportModalClose}
          />
        </PageContentBodyConnector>
      </PageContent>
    );
  }
}

SeriesDetails.propTypes = {
  id: PropTypes.number.isRequired,
  tvdbId: PropTypes.number.isRequired,
  tvMazeId: PropTypes.number,
  imdbId: PropTypes.string,
  title: PropTypes.string.isRequired,
  runtime: PropTypes.number.isRequired,
  ratings: PropTypes.object.isRequired,
  path: PropTypes.string.isRequired,
  statistics: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  network: PropTypes.string,
  overview: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  seasons: PropTypes.arrayOf(PropTypes.object).isRequired,
  alternateTitles: PropTypes.arrayOf(PropTypes.string).isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  isSaving: PropTypes.bool.isRequired,
  isRefreshing: PropTypes.bool.isRequired,
  isSearching: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  episodesError: PropTypes.object,
  episodeFilesError: PropTypes.object,
  hasMonitoredEpisodes: PropTypes.bool.isRequired,
  previousSeries: PropTypes.object.isRequired,
  nextSeries: PropTypes.object.isRequired,
  onMonitorTogglePress: PropTypes.func.isRequired,
  onRefreshPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

SeriesDetails.defaultProps = {
  isSaving: false
};

export default SeriesDetails;
