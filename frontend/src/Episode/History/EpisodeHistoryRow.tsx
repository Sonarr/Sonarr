import React, { useCallback, useState } from 'react';
import HistoryDetails from 'Activity/History/Details/HistoryDetails';
import HistoryEventTypeCell from 'Activity/History/HistoryEventTypeCell';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import EpisodeFormats from 'Episode/EpisodeFormats';
import EpisodeLanguages from 'Episode/EpisodeLanguages';
import EpisodeQuality from 'Episode/EpisodeQuality';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
import CustomFormat from 'typings/CustomFormat';
import { HistoryData, HistoryEventType } from 'typings/History';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import translate from 'Utilities/String/translate';
import styles from './EpisodeHistoryRow.css';

function getTitle(eventType: HistoryEventType) {
  switch (eventType) {
    case 'grabbed':
      return 'Grabbed';
    case 'seriesFolderImported':
      return 'Series Folder Imported';
    case 'downloadFolderImported':
      return 'Download Folder Imported';
    case 'downloadFailed':
      return 'Download Failed';
    case 'episodeFileDeleted':
      return 'Episode File Deleted';
    case 'episodeFileRenamed':
      return 'Episode File Renamed';
    default:
      return 'Unknown';
  }
}

interface EpisodeHistoryRowProps {
  id: number;
  eventType: HistoryEventType;
  sourceTitle: string;
  languages: Language[];
  quality: QualityModel;
  qualityCutoffNotMet: boolean;
  customFormats: CustomFormat[];
  customFormatScore: number;
  date: string;
  data: HistoryData;
  downloadId?: string;
  onMarkAsFailedPress: (id: number) => void;
}

function EpisodeHistoryRow({
  id,
  eventType,
  sourceTitle,
  languages,
  quality,
  qualityCutoffNotMet,
  customFormats,
  customFormatScore,
  date,
  data,
  downloadId,
  onMarkAsFailedPress,
}: EpisodeHistoryRowProps) {
  const [isMarkAsFailedModalOpen, setIsMarkAsFailedModalOpen] = useState(false);

  const handleMarkAsFailedPress = useCallback(() => {
    setIsMarkAsFailedModalOpen(true);
  }, []);

  const handleConfirmMarkAsFailed = useCallback(() => {
    onMarkAsFailedPress(id);
    setIsMarkAsFailedModalOpen(false);
  }, [id, onMarkAsFailedPress]);

  const handleMarkAsFailedModalClose = useCallback(() => {
    setIsMarkAsFailedModalOpen(false);
  }, []);

  return (
    <TableRow>
      <HistoryEventTypeCell eventType={eventType} data={data} />

      <TableRowCell>{sourceTitle}</TableRowCell>

      <TableRowCell>
        <EpisodeLanguages languages={languages} />
      </TableRowCell>

      <TableRowCell>
        <EpisodeQuality
          quality={quality}
          isCutoffNotMet={qualityCutoffNotMet}
        />
      </TableRowCell>

      <TableRowCell>
        <EpisodeFormats formats={customFormats} />
      </TableRowCell>

      <TableRowCell>
        {formatCustomFormatScore(customFormatScore, customFormats.length)}
      </TableRowCell>

      <RelativeDateCell date={date} includeSeconds={true} includeTime={true} />

      <TableRowCell className={styles.actions}>
        <Popover
          anchor={<Icon name={icons.INFO} />}
          title={getTitle(eventType)}
          body={
            <HistoryDetails
              eventType={eventType}
              sourceTitle={sourceTitle}
              data={data}
              downloadId={downloadId}
            />
          }
          position={tooltipPositions.LEFT}
        />

        {eventType === 'grabbed' && (
          <IconButton
            title={translate('MarkAsFailed')}
            name={icons.REMOVE}
            size={14}
            onPress={handleMarkAsFailedPress}
          />
        )}
      </TableRowCell>

      <ConfirmModal
        isOpen={isMarkAsFailedModalOpen}
        kind={kinds.DANGER}
        title={translate('MarkAsFailed')}
        message={translate('MarkAsFailedConfirmation', { sourceTitle })}
        confirmLabel={translate('MarkAsFailed')}
        onConfirm={handleConfirmMarkAsFailed}
        onCancel={handleMarkAsFailedModalClose}
      />
    </TableRow>
  );
}

export default EpisodeHistoryRow;
