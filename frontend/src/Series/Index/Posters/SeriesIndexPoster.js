import PropTypes from 'prop-types';
import React, { Component } from 'react';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import { icons } from 'Helpers/Props';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import SeriesPoster from 'Series/SeriesPoster';
import EditSeriesModalConnector from 'Series/Edit/EditSeriesModalConnector';
import DeleteSeriesModal from 'Series/Delete/DeleteSeriesModal';
import SeriesIndexProgressBar from 'Series/Index/ProgressBar/SeriesIndexProgressBar';
import SeriesIndexPosterInfo from './SeriesIndexPosterInfo';
import styles from './SeriesIndexPoster.css';

class SeriesIndexPoster extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPosterError: false,
      isEditSeriesModalOpen: false,
      isDeleteSeriesModalOpen: false
    };
  }

  //
  // Listeners

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

  onPosterLoad = () => {
    if (this.state.hasPosterError) {
      this.setState({ hasPosterError: false });
    }
  }

  onPosterLoadError = () => {
    if (!this.state.hasPosterError) {
      this.setState({ hasPosterError: true });
    }
  }

  //
  // Render

  render() {
    const {
      id,
      title,
      monitored,
      status,
      titleSlug,
      nextAiring,
      statistics,
      images,
      posterWidth,
      posterHeight,
      detailedProgressBar,
      showTitle,
      showMonitored,
      showQualityProfile,
      qualityProfile,
      showSearchAction,
      showRelativeDates,
      shortDateFormat,
      timeFormat,
      isRefreshingSeries,
      isSearchingSeries,
      onRefreshSeriesPress,
      onSearchPress,
      ...otherProps
    } = this.props;

    const {
      seasonCount,
      episodeCount,
      episodeFileCount,
      totalEpisodeCount,
      sizeOnDisk
    } = statistics;

    const {
      hasPosterError,
      isEditSeriesModalOpen,
      isDeleteSeriesModalOpen
    } = this.state;

    const link = `/series/${titleSlug}`;

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`
    };

    return (
      <div className={styles.content}>
        <div className={styles.posterContainer}>
          <Label className={styles.controls}>
            <SpinnerIconButton
              className={styles.action}
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
              className={styles.action}
              name={icons.EDIT}
              title="Edit Series"
              onPress={this.onEditSeriesPress}
            />
          </Label>

          {
            status === 'ended' &&
              <div
                className={styles.ended}
                title="Ended"
              />
          }

          <Link
            className={styles.link}
            style={elementStyle}
            to={link}
          >
            <SeriesPoster
              style={elementStyle}
              images={images}
              size={250}
              lazy={false}
              overflow={true}
              onError={this.onPosterLoadError}
              onLoad={this.onPosterLoad}
            />

            {
              hasPosterError &&
                <div className={styles.overlayTitle}>
                  {title}
                </div>
            }
          </Link>
        </div>

        <SeriesIndexProgressBar
          monitored={monitored}
          status={status}
          episodeCount={episodeCount}
          episodeFileCount={episodeFileCount}
          totalEpisodeCount={totalEpisodeCount}
          posterWidth={posterWidth}
          detailedProgressBar={detailedProgressBar}
        />

        {
          showTitle &&
            <div className={styles.title}>
              {title}
            </div>
        }

        {
          showMonitored &&
            <div className={styles.title}>
              {monitored ? 'Monitored' : 'Unmonitored'}
            </div>
        }

        {
          showQualityProfile &&
            <div className={styles.title}>
              {qualityProfile.name}
            </div>
        }

        {
          nextAiring &&
            <div className={styles.nextAiring}>
              {
                getRelativeDate(
                  nextAiring,
                  shortDateFormat,
                  showRelativeDates,
                  {
                    timeFormat,
                    timeForToday: true
                  }
                )
              }
            </div>
        }

        <SeriesIndexPosterInfo
          seasonCount={seasonCount}
          sizeOnDisk={sizeOnDisk}
          qualityProfile={qualityProfile}
          showQualityProfile={showQualityProfile}
          showRelativeDates={showRelativeDates}
          shortDateFormat={shortDateFormat}
          timeFormat={timeFormat}
          {...otherProps}
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
      </div>
    );
  }
}

SeriesIndexPoster.propTypes = {
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  nextAiring: PropTypes.string,
  statistics: PropTypes.object.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  posterWidth: PropTypes.number.isRequired,
  posterHeight: PropTypes.number.isRequired,
  detailedProgressBar: PropTypes.bool.isRequired,
  showTitle: PropTypes.bool.isRequired,
  showMonitored: PropTypes.bool.isRequired,
  showQualityProfile: PropTypes.bool.isRequired,
  qualityProfile: PropTypes.object.isRequired,
  showSearchAction: PropTypes.bool.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  isRefreshingSeries: PropTypes.bool.isRequired,
  isSearchingSeries: PropTypes.bool.isRequired,
  onRefreshSeriesPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

SeriesIndexPoster.defaultProps = {
  statistics: {
    seasonCount: 0,
    episodeCount: 0,
    episodeFileCount: 0,
    totalEpisodeCount: 0
  }
};

export default SeriesIndexPoster;
