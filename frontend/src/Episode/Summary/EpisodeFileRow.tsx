import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import EpisodeFormats from 'Episode/EpisodeFormats';
import EpisodeLanguages from 'Episode/EpisodeLanguages';
import EpisodeQuality from 'Episode/EpisodeQuality';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import CustomFormat from 'typings/CustomFormat';
import formatBytes from 'Utilities/Number/formatBytes';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import translate from 'Utilities/String/translate';
import MediaInfo from './MediaInfo';
import styles from './EpisodeFileRow.css';

interface EpisodeFileRowProps {
  path: string;
  size: number;
  languages: Language[];
  quality: QualityModel;
  qualityCutoffNotMet: boolean;
  customFormats: CustomFormat[];
  customFormatScore: number;
  mediaInfo: object;
  columns: Column[];
  onDeleteEpisodeFile(): void;
}

function EpisodeFileRow(props: EpisodeFileRowProps) {
  const {
    path,
    size,
    languages,
    quality,
    customFormats,
    customFormatScore,
    qualityCutoffNotMet,
    mediaInfo,
    columns,
    onDeleteEpisodeFile,
  } = props;

  const [
    isRemoveEpisodeFileModalOpen,
    setRemoveEpisodeFileModalOpen,
    setRemoveEpisodeFileModalClosed,
  ] = useModalOpenState(false);

  const handleRemoveEpisodeFilePress = useCallback(() => {
    onDeleteEpisodeFile();

    setRemoveEpisodeFileModalClosed();
  }, [onDeleteEpisodeFile, setRemoveEpisodeFileModalClosed]);

  return (
    <TableRow>
      {columns.map(({ name, isVisible }) => {
        if (!isVisible) {
          return null;
        }

        if (name === 'path') {
          return <TableRowCell key={name}>{path}</TableRowCell>;
        }

        if (name === 'size') {
          return <TableRowCell key={name}>{formatBytes(size)}</TableRowCell>;
        }

        if (name === 'languages') {
          return (
            <TableRowCell key={name} className={styles.languages}>
              <EpisodeLanguages languages={languages} />
            </TableRowCell>
          );
        }

        if (name === 'quality') {
          return (
            <TableRowCell key={name} className={styles.quality}>
              <EpisodeQuality
                quality={quality}
                isCutoffNotMet={qualityCutoffNotMet}
              />
            </TableRowCell>
          );
        }

        if (name === 'customFormats') {
          return (
            <TableRowCell key={name} className={styles.customFormats}>
              <EpisodeFormats formats={customFormats} />
            </TableRowCell>
          );
        }

        if (name === 'customFormatScore') {
          return (
            <TableRowCell key={name} className={styles.customFormatScore}>
              {formatCustomFormatScore(customFormatScore, customFormats.length)}
            </TableRowCell>
          );
        }

        if (name === 'actions') {
          return (
            <TableRowCell key={name} className={styles.actions}>
              {mediaInfo ? (
                <Popover
                  anchor={<Icon name={icons.MEDIA_INFO} />}
                  title={translate('MediaInfo')}
                  body={<MediaInfo {...mediaInfo} />}
                  position={tooltipPositions.LEFT}
                />
              ) : null}

              <IconButton
                title={translate('DeleteEpisodeFromDisk')}
                name={icons.REMOVE}
                onPress={setRemoveEpisodeFileModalOpen}
              />
            </TableRowCell>
          );
        }

        return null;
      })}

      <ConfirmModal
        isOpen={isRemoveEpisodeFileModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteEpisodeFile')}
        message={translate('DeleteEpisodeFileMessage', { path })}
        confirmLabel={translate('Delete')}
        onConfirm={handleRemoveEpisodeFilePress}
        onCancel={setRemoveEpisodeFileModalClosed}
      />
    </TableRow>
  );
}

export default EpisodeFileRow;
