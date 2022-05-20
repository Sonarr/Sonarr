import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import HeartRating from 'Components/HeartRating';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import ProgressBar from 'Components/ProgressBar';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import TagListConnector from 'Components/TagListConnector';
import { icons } from 'Helpers/Props';
import DeleteSeriesModal from 'Series/Delete/DeleteSeriesModal';
import EditSeriesModalConnector from 'Series/Edit/EditSeriesModalConnector';
import SeriesBanner from 'Series/SeriesBanner';
import SeriesTitleLink from 'Series/SeriesTitleLink';
import formatBytes from 'Utilities/Number/formatBytes';
import getProgressBarKind from 'Utilities/Series/getProgressBarKind';
import titleCase from 'Utilities/String/titleCase';
import hasGrowableColumns from './hasGrowableColumns';
import SeriesStatusCell from './SeriesStatusCell';
import styles from './SeriesIndexRow.css';

class SeriesIndexRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasBannerError: false,
      isEditSeriesModalOpen: false,
      isDeleteSeriesModalOpen: false
    };
  }

  onEditSeriesPress = () => {
    this.setState({ isEditSeriesModalOpen: true });
  };

  onEditSeriesModalClose = () => {
    this.setState({ isEditSeriesModalOpen: false });
  };

  onDeleteSeriesPress = () => {
    this.setState({
      isEditSeriesModalOpen: false,
      isDeleteSeriesModalOpen: true
    });
  };

  onDeleteSeriesModalClose = () => {
    this.setState({ isDeleteSeriesModalOpen: false });
  };

  onUseSceneNumberingChange = () => {
    // Mock handler to satisfy `onChange` being required for `CheckInput`.
    //
  };

  onBannerLoad = () => {
    if (this.state.hasBannerError) {
      this.setState({ hasBannerError: false });
    }
  };

  onBannerLoadError = () => {
    if (!this.state.hasBannerError) {
      this.setState({ hasBannerError: true });
    }
  };

  //
  // Render

  render() {
    const {
      id,
      monitored,
      status,
      title,
      titleSlug,
      seriesType,
      network,
      qualityProfile,
      languageProfile,
      nextAiring,
      previousAiring,
      added,
      statistics,
      latestSeason,
      year,
      path,
      genres,
      ratings,
      certification,
      tags,
      images,
      useSceneNumbering,
      showBanners,
      showSearchAction,
      columns,
      isRefreshingSeries,
      isSearchingSeries,
      onRefreshSeriesPress,
      onSearchPress
    } = this.props;

    const {
      seasonCount,
      episodeCount,
      episodeFileCount,
      totalEpisodeCount,
      releaseGroups,
      sizeOnDisk
    } = statistics;

    const {
      hasBannerError,
      isEditSeriesModalOpen,
      isDeleteSeriesModalOpen
    } = this.state;

    return (
      <>
        {
          columns.map((column) => {
            const {
              name,
              isVisible
            } = column;

            if (!isVisible) {
              return null;
            }

            if (name === 'status') {
              return (
                <SeriesStatusCell
                  key={name}
                  className={styles[name]}
                  monitored={monitored}
                  status={status}
                  component={VirtualTableRowCell}
                />
              );
            }

            if (name === 'sortTitle') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={classNames(
                    styles[name],
                    showBanners && styles.banner,
                    showBanners && !hasGrowableColumns(columns) && styles.bannerGrow
                  )}
                >
                  {
                    showBanners ?
                      <Link
                        className={styles.link}
                        to={`/series/${titleSlug}`}
                      >
                        <SeriesBanner
                          className={styles.bannerImage}
                          images={images}
                          lazy={false}
                          overflow={true}
                          onError={this.onBannerLoadError}
                          onLoad={this.onBannerLoad}
                        />

                        {
                          hasBannerError &&
                            <div className={styles.overlayTitle}>
                              {title}
                            </div>
                        }
                      </Link> :

                      <SeriesTitleLink
                        titleSlug={titleSlug}
                        title={title}
                      />
                  }
                </VirtualTableRowCell>
              );
            }

            if (name === 'seriesType') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {titleCase(seriesType)}
                </VirtualTableRowCell>
              );
            }

            if (name === 'network') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {network}
                </VirtualTableRowCell>
              );
            }

            if (name === 'qualityProfileId') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {qualityProfile.name}
                </VirtualTableRowCell>
              );
            }

            if (name === 'languageProfileId') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {languageProfile.name}
                </VirtualTableRowCell>
              );
            }

            if (name === 'nextAiring') {
              return (
                <RelativeDateCellConnector
                  key={name}
                  className={styles[name]}
                  date={nextAiring}
                  component={VirtualTableRowCell}
                />
              );
            }

            if (name === 'previousAiring') {
              return (
                <RelativeDateCellConnector
                  key={name}
                  className={styles[name]}
                  date={previousAiring}
                  component={VirtualTableRowCell}
                />
              );
            }

            if (name === 'added') {
              return (
                <RelativeDateCellConnector
                  key={name}
                  className={styles[name]}
                  date={added}
                  component={VirtualTableRowCell}
                />
              );
            }

            if (name === 'seasonCount') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {seasonCount}
                </VirtualTableRowCell>
              );
            }

            if (name === 'episodeProgress') {
              const progress = episodeCount ? episodeFileCount / episodeCount * 100 : 100;

              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <ProgressBar
                    progress={progress}
                    kind={getProgressBarKind(status, monitored, progress)}
                    showText={true}
                    text={`${episodeFileCount} / ${episodeCount}`}
                    title={`${episodeFileCount} / ${episodeCount} (Total: ${totalEpisodeCount})`}
                    width={125}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'latestSeason') {
              if (!latestSeason) {
                return (
                  <VirtualTableRowCell
                    key={name}
                    className={styles[name]}
                  />
                );
              }

              const seasonStatistics = latestSeason.statistics || {};
              const progress = seasonStatistics.episodeCount ?
                seasonStatistics.episodeFileCount / seasonStatistics.episodeCount * 100 :
                100;

              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <ProgressBar
                    progress={progress}
                    kind={getProgressBarKind(status, monitored, progress)}
                    showText={true}
                    text={`${seasonStatistics.episodeFileCount} / ${seasonStatistics.episodeCount}`}
                    title={`${seasonStatistics.episodeFileCount} / ${seasonStatistics.episodeCount} (Total: ${seasonStatistics.totalEpisodeCount})`}
                    width={125}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'episodeCount') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {totalEpisodeCount}
                </VirtualTableRowCell>
              );
            }

            if (name === 'year') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {year}
                </VirtualTableRowCell>
              );
            }

            if (name === 'path') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {path}
                </VirtualTableRowCell>
              );
            }

            if (name === 'sizeOnDisk') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {formatBytes(sizeOnDisk)}
                </VirtualTableRowCell>
              );
            }

            if (name === 'genres') {
              const joinedGenres = genres.join(', ');

              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <span title={joinedGenres}>
                    {joinedGenres}
                  </span>
                </VirtualTableRowCell>
              );
            }

            if (name === 'ratings') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <HeartRating
                    rating={ratings.value}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'certification') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  {certification}
                </VirtualTableRowCell>
              );
            }

            if (name === 'releaseGroups') {
              const joinedReleaseGroups = releaseGroups.join(', ');
              const truncatedReleaseGroups = releaseGroups.length > 3 ?
                `${releaseGroups.slice(0, 3).join(', ')}...` :
                joinedReleaseGroups;

              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <span title={joinedReleaseGroups}>
                    {truncatedReleaseGroups}
                  </span>
                </VirtualTableRowCell>
              );
            }

            if (name === 'tags') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <TagListConnector
                    tags={tags}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'useSceneNumbering') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <CheckInput
                    className={styles.checkInput}
                    name="useSceneNumbering"
                    value={useSceneNumbering}
                    isDisabled={true}
                    onChange={this.onUseSceneNumberingChange}
                  />
                </VirtualTableRowCell>
              );
            }

            if (name === 'actions') {
              return (
                <VirtualTableRowCell
                  key={name}
                  className={styles[name]}
                >
                  <SpinnerIconButton
                    name={icons.REFRESH}
                    title="Refresh series"
                    isSpinning={isRefreshingSeries}
                    onPress={onRefreshSeriesPress}
                  />

                  {
                    showSearchAction &&
                      <SpinnerIconButton
                        className={styles.action}
                        name={icons.SEARCH}
                        title="Search for monitored episodes"
                        isSpinning={isSearchingSeries}
                        onPress={onSearchPress}
                      />
                  }

                  <IconButton
                    name={icons.EDIT}
                    title="Edit Series"
                    onPress={this.onEditSeriesPress}
                  />
                </VirtualTableRowCell>
              );
            }

            return null;
          })
        }

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
      </>
    );
  }
}

SeriesIndexRow.propTypes = {
  id: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  seriesType: PropTypes.string.isRequired,
  network: PropTypes.string,
  qualityProfile: PropTypes.object.isRequired,
  languageProfile: PropTypes.object.isRequired,
  nextAiring: PropTypes.string,
  previousAiring: PropTypes.string,
  added: PropTypes.string,
  statistics: PropTypes.object.isRequired,
  latestSeason: PropTypes.object,
  year: PropTypes.number,
  path: PropTypes.string.isRequired,
  genres: PropTypes.arrayOf(PropTypes.string).isRequired,
  ratings: PropTypes.object.isRequired,
  certification: PropTypes.string,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  useSceneNumbering: PropTypes.bool.isRequired,
  showBanners: PropTypes.bool.isRequired,
  showSearchAction: PropTypes.bool.isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  isRefreshingSeries: PropTypes.bool.isRequired,
  isSearchingSeries: PropTypes.bool.isRequired,
  onRefreshSeriesPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

SeriesIndexRow.defaultProps = {
  statistics: {
    seasonCount: 0,
    episodeCount: 0,
    episodeFileCount: 0,
    totalEpisodeCount: 0,
    releaseGroups: []
  },
  genres: [],
  tags: []
};

export default SeriesIndexRow;
