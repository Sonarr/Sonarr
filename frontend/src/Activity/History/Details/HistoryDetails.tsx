import React from 'react';
import { useSelector } from 'react-redux';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import DescriptionListItemDescription from 'Components/DescriptionList/DescriptionListItemDescription';
import DescriptionListItemTitle from 'Components/DescriptionList/DescriptionListItemTitle';
import Link from 'Components/Link/Link';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import {
  DownloadFailedHistory,
  DownloadFolderImportedHistory,
  DownloadIgnoredHistory,
  EpisodeFileDeletedHistory,
  EpisodeFileRenamedHistory,
  GrabbedHistoryData,
  HistoryData,
  HistoryEventType,
} from 'typings/History';
import formatDateTime from 'Utilities/Date/formatDateTime';
import formatAge from 'Utilities/Number/formatAge';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import translate from 'Utilities/String/translate';
import styles from './HistoryDetails.css';

interface HistoryDetailsProps {
  eventType: HistoryEventType;
  sourceTitle: string;
  data: HistoryData;
  downloadId?: string;
  shortDateFormat: string;
  timeFormat: string;
}

function HistoryDetails(props: HistoryDetailsProps) {
  const { eventType, sourceTitle, data, downloadId } = props;

  const { shortDateFormat, timeFormat } = useSelector(
    createUISettingsSelector()
  );

  if (eventType === 'grabbed') {
    const {
      indexer,
      releaseGroup,
      seriesMatchType,
      customFormatScore,
      nzbInfoUrl,
      downloadClient,
      downloadClientName,
      age,
      ageHours,
      ageMinutes,
      publishedDate,
    } = data as GrabbedHistoryData;

    const downloadClientNameInfo = downloadClientName ?? downloadClient;

    return (
      <DescriptionList>
        <DescriptionListItem
          descriptionClassName={styles.description}
          title={translate('Name')}
          data={sourceTitle}
        />

        {indexer ? (
          <DescriptionListItem title={translate('Indexer')} data={indexer} />
        ) : null}

        {releaseGroup ? (
          <DescriptionListItem
            descriptionClassName={styles.description}
            title={translate('ReleaseGroup')}
            data={releaseGroup}
          />
        ) : null}

        {customFormatScore && customFormatScore !== '0' ? (
          <DescriptionListItem
            title={translate('CustomFormatScore')}
            data={formatCustomFormatScore(parseInt(customFormatScore))}
          />
        ) : null}

        {seriesMatchType ? (
          <DescriptionListItem
            descriptionClassName={styles.description}
            title={translate('SeriesMatchType')}
            data={seriesMatchType}
          />
        ) : null}

        {nzbInfoUrl ? (
          <span>
            <DescriptionListItemTitle>
              {translate('InfoUrl')}
            </DescriptionListItemTitle>

            <DescriptionListItemDescription>
              <Link to={nzbInfoUrl}>{nzbInfoUrl}</Link>
            </DescriptionListItemDescription>
          </span>
        ) : null}

        {downloadClientNameInfo ? (
          <DescriptionListItem
            title={translate('DownloadClient')}
            data={downloadClientNameInfo}
          />
        ) : null}

        {downloadId ? (
          <DescriptionListItem title={translate('GrabId')} data={downloadId} />
        ) : null}

        {age || ageHours || ageMinutes ? (
          <DescriptionListItem
            title={translate('AgeWhenGrabbed')}
            data={formatAge(age, ageHours, ageMinutes)}
          />
        ) : null}

        {publishedDate ? (
          <DescriptionListItem
            title={translate('PublishedDate')}
            data={formatDateTime(publishedDate, shortDateFormat, timeFormat, {
              includeSeconds: true,
            })}
          />
        ) : null}
      </DescriptionList>
    );
  }

  if (eventType === 'downloadFailed') {
    const { message } = data as DownloadFailedHistory;

    return (
      <DescriptionList>
        <DescriptionListItem
          descriptionClassName={styles.description}
          title={translate('Name')}
          data={sourceTitle}
        />

        {downloadId ? (
          <DescriptionListItem title={translate('GrabId')} data={downloadId} />
        ) : null}

        {message ? (
          <DescriptionListItem title={translate('Message')} data={message} />
        ) : null}
      </DescriptionList>
    );
  }

  if (eventType === 'downloadFolderImported') {
    const { customFormatScore, droppedPath, importedPath } =
      data as DownloadFolderImportedHistory;

    return (
      <DescriptionList>
        <DescriptionListItem
          descriptionClassName={styles.description}
          title={translate('Name')}
          data={sourceTitle}
        />

        {droppedPath ? (
          <DescriptionListItem
            descriptionClassName={styles.description}
            title={translate('Source')}
            data={droppedPath}
          />
        ) : null}

        {importedPath ? (
          <DescriptionListItem
            descriptionClassName={styles.description}
            title={translate('ImportedTo')}
            data={importedPath}
          />
        ) : null}

        {customFormatScore && customFormatScore !== '0' ? (
          <DescriptionListItem
            title={translate('CustomFormatScore')}
            data={formatCustomFormatScore(parseInt(customFormatScore))}
          />
        ) : null}
      </DescriptionList>
    );
  }

  if (eventType === 'episodeFileDeleted') {
    const { reason, customFormatScore } = data as EpisodeFileDeletedHistory;

    let reasonMessage = '';

    switch (reason) {
      case 'Manual':
        reasonMessage = translate('DeletedReasonManual');
        break;
      case 'MissingFromDisk':
        reasonMessage = translate('DeletedReasonEpisodeMissingFromDisk');
        break;
      case 'Upgrade':
        reasonMessage = translate('DeletedReasonUpgrade');
        break;
      default:
        reasonMessage = '';
    }

    return (
      <DescriptionList>
        <DescriptionListItem title={translate('Name')} data={sourceTitle} />

        <DescriptionListItem title={translate('Reason')} data={reasonMessage} />

        {customFormatScore && customFormatScore !== '0' ? (
          <DescriptionListItem
            title={translate('CustomFormatScore')}
            data={formatCustomFormatScore(parseInt(customFormatScore))}
          />
        ) : null}
      </DescriptionList>
    );
  }

  if (eventType === 'episodeFileRenamed') {
    const { sourcePath, sourceRelativePath, path, relativePath } =
      data as EpisodeFileRenamedHistory;

    return (
      <DescriptionList>
        <DescriptionListItem
          title={translate('SourcePath')}
          data={sourcePath}
        />

        <DescriptionListItem
          title={translate('SourceRelativePath')}
          data={sourceRelativePath}
        />

        <DescriptionListItem title={translate('DestinationPath')} data={path} />

        <DescriptionListItem
          title={translate('DestinationRelativePath')}
          data={relativePath}
        />
      </DescriptionList>
    );
  }

  if (eventType === 'downloadIgnored') {
    const { message } = data as DownloadIgnoredHistory;

    return (
      <DescriptionList>
        <DescriptionListItem
          descriptionClassName={styles.description}
          title={translate('Name')}
          data={sourceTitle}
        />

        {downloadId ? (
          <DescriptionListItem title={translate('GrabId')} data={downloadId} />
        ) : null}

        {message ? (
          <DescriptionListItem title={translate('Message')} data={message} />
        ) : null}
      </DescriptionList>
    );
  }

  return (
    <DescriptionList>
      <DescriptionListItem
        descriptionClassName={styles.description}
        title={translate('Name')}
        data={sourceTitle}
      />
    </DescriptionList>
  );
}

export default HistoryDetails;
