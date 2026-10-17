import classNames from 'classnames';
import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import ClipboardButton from 'Components/Link/ClipboardButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import Popover from 'Components/Tooltip/Popover';
import EpisodeFormats from 'Episode/EpisodeFormats';
import { EpisodeFile } from 'EpisodeFile/EpisodeFile';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons, kinds, sizes, tooltipPositions } from 'Helpers/Props';
import getMediaLanguageNames from 'Language/getMediaLanguageNames';
import useLanguageName from 'Language/useLanguageName';
import formatBytes from 'Utilities/Number/formatBytes';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import translate from 'Utilities/String/translate';
import MediaInfo from './MediaInfo';
import styles from './EpisodeFileCard.module.css';

interface EpisodeFileCardProps {
  episodeFile: EpisodeFile;
  onDeleteEpisodeFile(): void;
}

function getSeparatorIndex(path: string) {
  return Math.max(path.lastIndexOf('/'), path.lastIndexOf('\\'));
}

function getFileName(path: string) {
  return path.slice(getSeparatorIndex(path) + 1);
}

function getFolder(path: string) {
  const index = getSeparatorIndex(path);

  if (index === -1) {
    return '';
  }

  const isRoot = index === 0 || path[index - 1] === ':';

  return path.slice(0, isRoot ? index + 1 : index);
}

interface FileChipProps {
  label: string;
  className?: string;
  title?: string;
  children: React.ReactNode;
}

function FileChip({ label, className, title, children }: FileChipProps) {
  return (
    <span className={classNames(styles.chip, className)} title={title}>
      <span className={styles.chipLabel}>{label}</span>
      <span className={styles.chipValue}>{children}</span>
    </span>
  );
}

function EpisodeFileCard({
  episodeFile,
  onDeleteEpisodeFile,
}: EpisodeFileCardProps) {
  const {
    path,
    size,
    languages,
    quality,
    qualityCutoffNotMet,
    customFormats,
    customFormatScore,
    releaseGroup,
    mediaInfo,
  } = episodeFile;

  const getLanguageName = useLanguageName();

  const [
    isRemoveEpisodeFileModalOpen,
    setRemoveEpisodeFileModalOpen,
    setRemoveEpisodeFileModalClosed,
  ] = useModalOpenState(false);

  const handleRemoveEpisodeFilePress = useCallback(() => {
    onDeleteEpisodeFile();
    setRemoveEpisodeFileModalClosed();
  }, [onDeleteEpisodeFile, setRemoveEpisodeFileModalClosed]);

  const folder = getFolder(path);

  const video = [
    mediaInfo?.videoCodec,
    mediaInfo?.videoBitDepth ? `${mediaInfo.videoBitDepth}-bit` : null,
  ]
    .filter(Boolean)
    .join(' ');

  const audioStream = mediaInfo?.audioStreams?.[0];

  const audio = [audioStream?.codec, audioStream?.channels?.toFixed(1)]
    .filter(Boolean)
    .join(' ');

  const languageNames = languages.map(({ name }) => name).join(' · ');

  const subtitleNames = getMediaLanguageNames(
    (mediaInfo?.subtitleStreams ?? []).map(({ language }) => language),
    getLanguageName
  );

  const { revision } = quality;

  const revisionText = [
    revision.real > 0 ? translate('Real') : null,
    revision.isRepack ? translate('Repack') : null,
    !revision.isRepack && revision.version > 1 ? translate('Proper') : null,
  ]
    .filter(Boolean)
    .join(' ');

  const subtitles =
    subtitleNames.length > 3
      ? `${subtitleNames.slice(0, 3).join(' · ')} +${subtitleNames.length - 3}`
      : subtitleNames.join(' · ');

  const score = formatCustomFormatScore(
    customFormatScore,
    customFormats.length
  );

  return (
    <div className={styles.card}>
      <div className={styles.header}>
        <div className={styles.fileInfo}>
          <div className={styles.fileName}>{getFileName(path)}</div>

          <div className={styles.folder}>
            <span className={styles.folderIcon}>
              <Icon name={icons.FOLDER} size={13} />
            </span>

            <span className={styles.folderPath}>{folder}</span>

            <ClipboardButton
              className={styles.copyButton}
              value={path}
              kind={kinds.DEFAULT}
            />
          </div>
        </div>

        <div className={styles.actions}>
          {mediaInfo ? (
            <Popover
              anchor={
                <span className={styles.mediaInfoAnchor}>
                  <Icon name={icons.MEDIA_INFO} size={13} />
                  {translate('MediaInfo')}
                </span>
              }
              title={translate('MediaInfo')}
              body={<MediaInfo {...mediaInfo} />}
              position={tooltipPositions.LEFT}
            />
          ) : null}

          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            size={sizes.SMALL}
            outline={true}
            title={translate('DeleteEpisodeFromDisk')}
            aria-label={translate('DeleteEpisodeFromDisk')}
            onPress={setRemoveEpisodeFileModalOpen}
          >
            <Icon name={icons.DELETE} size={13} />
          </Button>
        </div>
      </div>

      <div className={styles.chips}>
        <FileChip label={translate('Size')}>{formatBytes(size)}</FileChip>

        <FileChip
          label={translate('Quality')}
          title={
            qualityCutoffNotMet ? translate('QualityCutoffNotMet') : undefined
          }
        >
          {quality.quality.name}
          {revisionText ? (
            <span className={styles.revision}> {revisionText}</span>
          ) : null}
          {qualityCutoffNotMet ? (
            <Icon
              className={styles.cutoffIcon}
              name={icons.UPGRADE}
              size={12}
            />
          ) : null}
        </FileChip>

        {video ? <FileChip label={translate('Video')}>{video}</FileChip> : null}

        {audio ? <FileChip label={translate('Audio')}>{audio}</FileChip> : null}

        {languageNames ? (
          <FileChip label={translate('Languages')}>{languageNames}</FileChip>
        ) : null}

        {subtitles ? (
          <FileChip
            label={translate('Subtitles')}
            title={
              subtitleNames.length > 3 ? subtitleNames.join(', ') : undefined
            }
          >
            {subtitles}
          </FileChip>
        ) : null}

        {releaseGroup ? (
          <FileChip label={translate('Group')}>{releaseGroup}</FileChip>
        ) : null}
      </div>

      {customFormats.length ? (
        <div className={styles.formats}>
          <span className={styles.formatsLabel}>
            {translate('CustomFormats')}
          </span>

          <span
            className={classNames(
              styles.scoreValue,
              customFormatScore > 0 && styles.positive,
              customFormatScore < 0 && styles.negative
            )}
            title={translate('CustomFormatScore')}
          >
            {score}
          </span>

          <div className={styles.formatList}>
            <EpisodeFormats formats={customFormats} />
          </div>
        </div>
      ) : null}

      <ConfirmModal
        isOpen={isRemoveEpisodeFileModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteEpisodeFile')}
        message={translate('DeleteEpisodeFileMessage', { path })}
        confirmLabel={translate('Delete')}
        onConfirm={handleRemoveEpisodeFilePress}
        onCancel={setRemoveEpisodeFileModalClosed}
      />
    </div>
  );
}

export default EpisodeFileCard;
