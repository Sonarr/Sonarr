import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextTruncate from 'react-text-truncate';
import { icons } from 'Helpers/Props';
import dimensions from 'Styles/Variables/dimensions';
import fonts from 'Styles/Variables/fonts';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import SeriesPoster from 'Series/SeriesPoster';
import EditSeriesModalConnector from 'Series/Edit/EditSeriesModalConnector';
import DeleteSeriesModal from 'Series/Delete/DeleteSeriesModal';
import SeriesIndexProgressBar from 'Series/Index/ProgressBar/SeriesIndexProgressBar';
import SeriesIndexOverviewInfo from './SeriesIndexOverviewInfo';
import styles from './SeriesIndexOverview.css';

const columnPadding = parseInt(dimensions.seriesIndexColumnPadding);
const columnPaddingSmallScreen = parseInt(dimensions.seriesIndexColumnPaddingSmallScreen);
const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

// Hardcoded height beased on line-height of 32 + bottom margin of 10.
// Less side-effecty than using react-measure.
const titleRowHeight = 42;

function getContentHeight(rowHeight, isSmallScreen) {
  const padding = isSmallScreen ? columnPaddingSmallScreen : columnPadding;

  return rowHeight - padding;
}

class SeriesIndexOverview extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
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

  //
  // Render

  render() {
    const {
      style,
      id,
      title,
      overview,
      monitored,
      status,
      titleSlug,
      nextAiring,
      statistics,
      images,
      posterWidth,
      posterHeight,
      qualityProfile,
      overviewOptions,
      showRelativeDates,
      shortDateFormat,
      timeFormat,
      rowHeight,
      isSmallScreen,
      isRefreshingSeries,
      onRefreshSeriesPress,
      ...otherProps
    } = this.props;

    const {
      seasonCount,
      episodeCount,
      episodeFileCount,
      totalEpisodeCount
    } = statistics;

    const {
      isEditSeriesModalOpen,
      isDeleteSeriesModalOpen
    } = this.state;

    const link = `/series/${titleSlug}`;

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`
    };

    const contentHeight = getContentHeight(rowHeight, isSmallScreen);
    const overviewHeight = contentHeight - titleRowHeight;

    return (
      <div className={styles.container} style={style}>
        <div className={styles.content}>
          <div className={styles.poster}>
            <div className={styles.posterContainer}>
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
                  className={styles.poster}
                  style={elementStyle}
                  images={images}
                  size={250}
                  lazy={false}
                  overflow={true}
                />
              </Link>
            </div>

            <SeriesIndexProgressBar
              monitored={monitored}
              status={status}
              episodeCount={episodeCount}
              episodeFileCount={episodeFileCount}
              totalEpisodeCount={totalEpisodeCount}
              posterWidth={posterWidth}
              detailedProgressBar={overviewOptions.detailedProgressBar}
            />
          </div>

          <div className={styles.info} style={{ maxHeight: contentHeight }}>
            <div className={styles.titleRow}>
              <Link
                className={styles.title}
                to={link}
              >
                {title}
              </Link>

              <div className={styles.actions}>
                <SpinnerIconButton
                  name={icons.REFRESH}
                  title="Refresh series"
                  isSpinning={isRefreshingSeries}
                  onPress={onRefreshSeriesPress}
                />

                <IconButton
                  name={icons.EDIT}
                  title="Edit Series"
                  onPress={this.onEditSeriesPress}
                />
              </div>
            </div>

            <div className={styles.details}>
              <Link
                className={styles.overview}
                to={link}
              >
                <TextTruncate
                  line={Math.floor(overviewHeight / (defaultFontSize * lineHeight))}
                  text={overview}
                />
              </Link>

              <SeriesIndexOverviewInfo
                height={overviewHeight}
                monitored={monitored}
                nextAiring={nextAiring}
                seasonCount={seasonCount}
                qualityProfile={qualityProfile}
                showRelativeDates={showRelativeDates}
                shortDateFormat={shortDateFormat}
                timeFormat={timeFormat}
                {...overviewOptions}
                {...otherProps}
              />
            </div>
          </div>
        </div>

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

SeriesIndexOverview.propTypes = {
  style: PropTypes.object.isRequired,
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  overview: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  nextAiring: PropTypes.string,
  statistics: PropTypes.object.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  posterWidth: PropTypes.number.isRequired,
  posterHeight: PropTypes.number.isRequired,
  rowHeight: PropTypes.number.isRequired,
  qualityProfile: PropTypes.object.isRequired,
  overviewOptions: PropTypes.object.isRequired,
  showRelativeDates: PropTypes.bool.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  isRefreshingSeries: PropTypes.bool.isRequired,
  onRefreshSeriesPress: PropTypes.func.isRequired
};

SeriesIndexOverview.defaultProps = {
  episodeCount: 0,
  episodeFileCount: 0
};

export default SeriesIndexOverview;
