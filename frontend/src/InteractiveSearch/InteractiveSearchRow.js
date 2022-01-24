import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ProtocolLabel from 'Activity/Queue/ProtocolLabel';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import EpisodeLanguage from 'Episode/EpisodeLanguage';
import EpisodeQuality from 'Episode/EpisodeQuality';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import formatDateTime from 'Utilities/Date/formatDateTime';
import formatAge from 'Utilities/Number/formatAge';
import formatBytes from 'Utilities/Number/formatBytes';
import formatPreferredWordScore from 'Utilities/Number/formatPreferredWordScore';
import Peers from './Peers';
import ReleaseSceneIndicator from './ReleaseSceneIndicator';
import styles from './InteractiveSearchRow.css';

function getDownloadIcon(isGrabbing, isGrabbed, grabError) {
  if (isGrabbing) {
    return icons.SPINNER;
  } else if (isGrabbed) {
    return icons.DOWNLOADING;
  } else if (grabError) {
    return icons.DOWNLOADING;
  }

  return icons.DOWNLOAD;
}

function getDownloadTooltip(isGrabbing, isGrabbed, grabError) {
  if (isGrabbing) {
    return '';
  } else if (isGrabbed) {
    return 'Added to download queue';
  } else if (grabError) {
    return grabError;
  }

  return 'Add to download queue';
}

class InteractiveSearchRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isConfirmGrabModalOpen: false
    };
  }

  //
  // Listeners

  onGrabPress = () => {
    const {
      guid,
      indexerId,
      onGrabPress
    } = this.props;

    onGrabPress({
      guid,
      indexerId
    });
  };

  onConfirmGrabPress = () => {
    this.setState({ isConfirmGrabModalOpen: true });
  };

  onGrabConfirm = () => {
    this.setState({ isConfirmGrabModalOpen: false });

    const {
      guid,
      indexerId,
      searchPayload,
      onGrabPress
    } = this.props;

    onGrabPress({
      guid,
      indexerId,
      ...searchPayload
    });
  };

  onGrabCancel = () => {
    this.setState({ isConfirmGrabModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      protocol,
      age,
      ageHours,
      ageMinutes,
      publishDate,
      title,
      infoUrl,
      indexer,
      size,
      seeders,
      leechers,
      quality,
      language,
      customFormatScore,
      customFormats,
      sceneMapping,
      seasonNumber,
      episodeNumbers,
      absoluteEpisodeNumbers,
      mappedSeasonNumber,
      mappedEpisodeNumbers,
      mappedAbsoluteEpisodeNumbers,
      rejections,
      episodeRequested,
      downloadAllowed,
      isDaily,
      isGrabbing,
      isGrabbed,
      longDateFormat,
      timeFormat,
      grabError
    } = this.props;

    return (
      <TableRow>
        <TableRowCell className={styles.protocol}>
          <ProtocolLabel
            protocol={protocol}
          />
        </TableRowCell>

        <TableRowCell
          className={styles.age}
          title={formatDateTime(publishDate, longDateFormat, timeFormat, { includeSeconds: true })}
        >
          {formatAge(age, ageHours, ageMinutes)}
        </TableRowCell>

        <TableRowCell className={styles.title}>
          <Link to={infoUrl}>
            {title}
          </Link>
          <ReleaseSceneIndicator
            className={styles.sceneMapping}
            seasonNumber={mappedSeasonNumber}
            episodeNumbers={mappedEpisodeNumbers}
            absoluteEpisodeNumbers={mappedAbsoluteEpisodeNumbers}
            sceneSeasonNumber={seasonNumber}
            sceneEpisodeNumbers={episodeNumbers}
            sceneAbsoluteEpisodeNumbers={absoluteEpisodeNumbers}
            sceneMapping={sceneMapping}
            episodeRequested={episodeRequested}
            isDaily={isDaily}
          />
        </TableRowCell>

        <TableRowCell className={styles.indexer}>
          {indexer}
        </TableRowCell>

        <TableRowCell className={styles.size}>
          {formatBytes(size)}
        </TableRowCell>

        <TableRowCell className={styles.peers}>
          {
            protocol === 'torrent' &&
              <Peers
                seeders={seeders}
                leechers={leechers}
              />
          }
        </TableRowCell>

        <TableRowCell className={styles.language}>
          <EpisodeLanguage language={language} />
        </TableRowCell>

        <TableRowCell className={styles.quality}>
          <EpisodeQuality quality={quality} />
        </TableRowCell>

        <TableRowCell className={styles.customFormatScore}>
          <Tooltip
            anchor={
              formatPreferredWordScore(customFormatScore)
            }
            tooltip={
              <div>
                {
                  customFormats.map((format) => {
                    return (
                      <Label key={format.id}>
                        {format.name}
                      </Label>
                    );
                  })
                }
              </div>
            }
            position={tooltipPositions.BOTTOM}
          />
        </TableRowCell>

        <TableRowCell className={styles.rejected}>
          {
            !!rejections.length &&
              <Popover
                anchor={
                  <Icon
                    name={icons.DANGER}
                    kind={kinds.DANGER}
                  />
                }
                title="Release Rejected"
                body={
                  <ul>
                    {
                      rejections.map((rejection, index) => {
                        return (
                          <li key={index}>
                            {rejection}
                          </li>
                        );
                      })
                    }
                  </ul>
                }
                position={tooltipPositions.LEFT}
              />
          }
        </TableRowCell>

        <TableRowCell className={styles.download}>
          <SpinnerIconButton
            name={getDownloadIcon(isGrabbing, isGrabbed, grabError)}
            kind={grabError ? kinds.DANGER : kinds.DEFAULT}
            title={getDownloadTooltip(isGrabbing, isGrabbed, grabError)}
            isSpinning={isGrabbing}
            onPress={downloadAllowed ? this.onGrabPress : this.onConfirmGrabPress}
          />
        </TableRowCell>

        <ConfirmModal
          isOpen={this.state.isConfirmGrabModalOpen}
          kind={kinds.WARNING}
          title="Grab Release"
          message={`Sonarr was unable to determine which series and episode this release was for. Sonarr may be unable to automatically import this release. Do you want to grab '${title}'?`}
          confirmLabel="Grab"
          onConfirm={this.onGrabConfirm}
          onCancel={this.onGrabCancel}
        />
      </TableRow>
    );
  }
}

InteractiveSearchRow.propTypes = {
  guid: PropTypes.string.isRequired,
  protocol: PropTypes.string.isRequired,
  age: PropTypes.number.isRequired,
  ageHours: PropTypes.number.isRequired,
  ageMinutes: PropTypes.number.isRequired,
  publishDate: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  infoUrl: PropTypes.string.isRequired,
  indexerId: PropTypes.number.isRequired,
  indexer: PropTypes.string.isRequired,
  size: PropTypes.number.isRequired,
  seeders: PropTypes.number,
  leechers: PropTypes.number,
  quality: PropTypes.object.isRequired,
  language: PropTypes.object.isRequired,
  customFormats: PropTypes.arrayOf(PropTypes.object),
  customFormatScore: PropTypes.number.isRequired,
  sceneMapping: PropTypes.object,
  seasonNumber: PropTypes.number,
  episodeNumbers: PropTypes.arrayOf(PropTypes.number),
  absoluteEpisodeNumbers: PropTypes.arrayOf(PropTypes.number),
  mappedSeasonNumber: PropTypes.number,
  mappedEpisodeNumbers: PropTypes.arrayOf(PropTypes.number),
  mappedAbsoluteEpisodeNumbers: PropTypes.arrayOf(PropTypes.number),
  rejections: PropTypes.arrayOf(PropTypes.string).isRequired,
  episodeRequested: PropTypes.bool.isRequired,
  downloadAllowed: PropTypes.bool.isRequired,
  isDaily: PropTypes.bool.isRequired,
  isGrabbing: PropTypes.bool.isRequired,
  isGrabbed: PropTypes.bool.isRequired,
  grabError: PropTypes.string,
  longDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  searchPayload: PropTypes.object.isRequired,
  onGrabPress: PropTypes.func.isRequired
};

InteractiveSearchRow.defaultProps = {
  rejections: [],
  isGrabbing: false,
  isGrabbed: false
};

export default InteractiveSearchRow;
