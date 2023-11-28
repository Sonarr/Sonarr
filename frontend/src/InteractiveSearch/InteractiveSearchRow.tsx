import React, { useCallback, useState } from 'react';
import ProtocolLabel from 'Activity/Queue/ProtocolLabel';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import type DownloadProtocol from 'DownloadClient/DownloadProtocol';
import EpisodeFormats from 'Episode/EpisodeFormats';
import EpisodeLanguages from 'Episode/EpisodeLanguages';
import EpisodeQuality from 'Episode/EpisodeQuality';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import CustomFormat from 'typings/CustomFormat';
import formatDateTime from 'Utilities/Date/formatDateTime';
import formatAge from 'Utilities/Number/formatAge';
import formatBytes from 'Utilities/Number/formatBytes';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import translate from 'Utilities/String/translate';
import OverrideMatchModal from './OverrideMatch/OverrideMatchModal';
import Peers from './Peers';
import ReleaseEpisode from './ReleaseEpisode';
import ReleaseSceneIndicator from './ReleaseSceneIndicator';
import styles from './InteractiveSearchRow.css';

function getDownloadIcon(
  isGrabbing: boolean,
  isGrabbed: boolean,
  grabError?: string
) {
  if (isGrabbing) {
    return icons.SPINNER;
  } else if (isGrabbed) {
    return icons.DOWNLOADING;
  } else if (grabError) {
    return icons.DOWNLOADING;
  }

  return icons.DOWNLOAD;
}

function getDownloadKind(isGrabbed: boolean, grabError?: string) {
  if (isGrabbed) {
    return kinds.SUCCESS;
  }

  if (grabError) {
    return kinds.DANGER;
  }

  return kinds.DEFAULT;
}

function getDownloadTooltip(
  isGrabbing: boolean,
  isGrabbed: boolean,
  grabError?: string
) {
  if (isGrabbing) {
    return '';
  } else if (isGrabbed) {
    return translate('AddedToDownloadQueue');
  } else if (grabError) {
    return grabError;
  }

  return translate('AddToDownloadQueue');
}

interface InteractiveSearchRowProps {
  guid: string;
  protocol: DownloadProtocol;
  age: number;
  ageHours: number;
  ageMinutes: number;
  publishDate: string;
  title: string;
  infoUrl: string;
  indexerId: number;
  indexer: string;
  size: number;
  seeders?: number;
  leechers?: number;
  quality: QualityModel;
  languages: Language[];
  customFormats: CustomFormat[];
  customFormatScore: number;
  sceneMapping?: object;
  seasonNumber?: number;
  episodeNumbers?: number[];
  absoluteEpisodeNumbers?: number[];
  mappedSeriesId?: number;
  mappedSeasonNumber?: number;
  mappedEpisodeNumbers?: number[];
  mappedAbsoluteEpisodeNumbers?: number[];
  mappedEpisodeInfo: ReleaseEpisode[];
  rejections: string[];
  episodeRequested: boolean;
  downloadAllowed: boolean;
  isDaily: boolean;
  isGrabbing: boolean;
  isGrabbed: boolean;
  grabError?: string;
  longDateFormat: string;
  timeFormat: string;
  searchPayload: object;
  onGrabPress(...args: unknown[]): void;
}

function InteractiveSearchRow(props: InteractiveSearchRowProps) {
  const {
    guid,
    indexerId,
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
    languages,
    customFormatScore,
    customFormats,
    sceneMapping,
    seasonNumber,
    episodeNumbers,
    absoluteEpisodeNumbers,
    mappedSeriesId,
    mappedSeasonNumber,
    mappedEpisodeNumbers,
    mappedAbsoluteEpisodeNumbers,
    mappedEpisodeInfo,
    rejections = [],
    episodeRequested,
    downloadAllowed,
    isDaily,
    isGrabbing = false,
    isGrabbed = false,
    longDateFormat,
    timeFormat,
    grabError,
    searchPayload,
    onGrabPress,
  } = props;

  const [isConfirmGrabModalOpen, setIsConfirmGrabModalOpen] = useState(false);
  const [isOverrideModalOpen, setIsOverrideModalOpen] = useState(false);

  const onGrabPressWrapper = useCallback(() => {
    if (downloadAllowed) {
      onGrabPress({
        guid,
        indexerId,
      });

      return;
    }

    setIsConfirmGrabModalOpen(true);
  }, [
    guid,
    indexerId,
    downloadAllowed,
    onGrabPress,
    setIsConfirmGrabModalOpen,
  ]);

  const onGrabConfirm = useCallback(() => {
    setIsConfirmGrabModalOpen(false);

    onGrabPress({
      guid,
      indexerId,
      ...searchPayload,
    });
  }, [guid, indexerId, searchPayload, onGrabPress, setIsConfirmGrabModalOpen]);

  const onGrabCancel = useCallback(() => {
    setIsConfirmGrabModalOpen(false);
  }, [setIsConfirmGrabModalOpen]);

  const onOverridePress = useCallback(() => {
    setIsOverrideModalOpen(true);
  }, [setIsOverrideModalOpen]);

  const onOverrideModalClose = useCallback(() => {
    setIsOverrideModalOpen(false);
  }, [setIsOverrideModalOpen]);

  return (
    <TableRow>
      <TableRowCell className={styles.protocol}>
        <ProtocolLabel protocol={protocol} />
      </TableRowCell>

      <TableRowCell
        className={styles.age}
        title={formatDateTime(publishDate, longDateFormat, timeFormat, {
          includeSeconds: true,
        })}
      >
        {formatAge(age, ageHours, ageMinutes)}
      </TableRowCell>

      <TableRowCell>
        <div className={styles.titleContent}>
          <Link to={infoUrl}>{title}</Link>
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
        </div>
      </TableRowCell>

      <TableRowCell className={styles.indexer}>{indexer}</TableRowCell>

      <TableRowCell className={styles.size}>{formatBytes(size)}</TableRowCell>

      <TableRowCell className={styles.peers}>
        {protocol === 'torrent' ? (
          <Peers seeders={seeders} leechers={leechers} />
        ) : null}
      </TableRowCell>

      <TableRowCell className={styles.languages}>
        <EpisodeLanguages languages={languages} />
      </TableRowCell>

      <TableRowCell className={styles.quality}>
        <EpisodeQuality quality={quality} showRevision={true} />
      </TableRowCell>

      <TableRowCell className={styles.customFormatScore}>
        <Tooltip
          anchor={formatCustomFormatScore(
            customFormatScore,
            customFormats.length
          )}
          tooltip={<EpisodeFormats formats={customFormats} />}
          position={tooltipPositions.BOTTOM}
        />
      </TableRowCell>

      <TableRowCell className={styles.rejected}>
        {rejections.length ? (
          <Popover
            anchor={<Icon name={icons.DANGER} kind={kinds.DANGER} />}
            title={translate('ReleaseRejected')}
            body={
              <ul>
                {rejections.map((rejection, index) => {
                  return <li key={index}>{rejection}</li>;
                })}
              </ul>
            }
            position={tooltipPositions.LEFT}
          />
        ) : null}
      </TableRowCell>

      <TableRowCell className={styles.download}>
        <SpinnerIconButton
          name={getDownloadIcon(isGrabbing, isGrabbed, grabError)}
          kind={getDownloadKind(isGrabbed, grabError)}
          title={getDownloadTooltip(isGrabbing, isGrabbed, grabError)}
          isSpinning={isGrabbing}
          onPress={onGrabPressWrapper}
        />

        <Link
          className={styles.manualDownloadContent}
          title={translate('OverrideAndAddToDownloadQueue')}
          onPress={onOverridePress}
        >
          <div className={styles.manualDownloadContent}>
            <Icon
              className={styles.interactiveIcon}
              name={icons.INTERACTIVE}
              size={12}
            />

            <Icon
              className={styles.downloadIcon}
              name={icons.CIRCLE_DOWN}
              size={10}
            />
          </div>
        </Link>
      </TableRowCell>

      <ConfirmModal
        isOpen={isConfirmGrabModalOpen}
        kind={kinds.WARNING}
        title={translate('GrabRelease')}
        message={translate('GrabReleaseUnknownSeriesOrEpisodeMessageText', {
          title,
        })}
        confirmLabel={translate('Grab')}
        onConfirm={onGrabConfirm}
        onCancel={onGrabCancel}
      />

      <OverrideMatchModal
        isOpen={isOverrideModalOpen}
        title={title}
        indexerId={indexerId}
        guid={guid}
        seriesId={mappedSeriesId}
        seasonNumber={mappedSeasonNumber}
        episodes={mappedEpisodeInfo}
        languages={languages}
        quality={quality}
        protocol={protocol}
        isGrabbing={isGrabbing}
        grabError={grabError}
        onModalClose={onOverrideModalClose}
      />
    </TableRow>
  );
}

export default InteractiveSearchRow;
