import PropTypes from 'prop-types';
import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import DescriptionListItemDescription from 'Components/DescriptionList/DescriptionListItemDescription';
import DescriptionListItemTitle from 'Components/DescriptionList/DescriptionListItemTitle';
import Link from 'Components/Link/Link';
import formatDateTime from 'Utilities/Date/formatDateTime';
import formatAge from 'Utilities/Number/formatAge';
import formatPreferredWordScore from 'Utilities/Number/formatPreferredWordScore';
import styles from './HistoryDetails.css';

function HistoryDetails(props) {
  const {
    eventType,
    sourceTitle,
    data,
    shortDateFormat,
    timeFormat
  } = props;

  if (eventType === 'grabbed') {
    const {
      indexer,
      releaseGroup,
      preferredWordScore,
      seriesMatchType,
      nzbInfoUrl,
      downloadClient,
      downloadClientName,
      downloadId,
      age,
      ageHours,
      ageMinutes,
      publishedDate
    } = data;

    const downloadClientNameInfo = downloadClientName ?? downloadClient;

    return (
      <DescriptionList>
        <DescriptionListItem
          descriptionClassName={styles.description}
          title="Name"
          data={sourceTitle}
        />

        {
          indexer ?
            <DescriptionListItem
              title="Indexer"
              data={indexer}
            /> :
            null
        }

        {
          releaseGroup ?
            <DescriptionListItem
              descriptionClassName={styles.description}
              title="Release Group"
              data={releaseGroup}
            /> :
            null
        }

        {
          preferredWordScore && preferredWordScore !== '0' ?
            <DescriptionListItem
              title="Preferred Word Score"
              data={formatPreferredWordScore(preferredWordScore)}
            /> :
            null
        }

        {
          seriesMatchType ?
            <DescriptionListItem
              descriptionClassName={styles.description}
              title="Series Match Type"
              data={seriesMatchType}
            /> :
            null
        }

        {
          nzbInfoUrl ?
            <span>
              <DescriptionListItemTitle>
                Info URL
              </DescriptionListItemTitle>

              <DescriptionListItemDescription>
                <Link to={nzbInfoUrl}>{nzbInfoUrl}</Link>
              </DescriptionListItemDescription>
            </span> :
            null
        }

        {
          downloadClientNameInfo ?
            <DescriptionListItem
              title="Download Client"
              data={downloadClientNameInfo}
            /> :
            null
        }

        {
          downloadId ?
            <DescriptionListItem
              title="Grab ID"
              data={downloadId}
            /> :
            null
        }

        {
          age || ageHours || ageMinutes ?
            <DescriptionListItem
              title="Age (when grabbed)"
              data={formatAge(age, ageHours, ageMinutes)}
            /> :
            null
        }

        {
          publishedDate ?
            <DescriptionListItem
              title="Published Date"
              data={formatDateTime(publishedDate, shortDateFormat, timeFormat, { includeSeconds: true })}
            /> :
            null
        }
      </DescriptionList>
    );
  }

  if (eventType === 'downloadFailed') {
    const {
      message
    } = data;

    return (
      <DescriptionList>
        <DescriptionListItem
          descriptionClassName={styles.description}
          title="Name"
          data={sourceTitle}
        />

        {
          message ?
            <DescriptionListItem
              title="Message"
              data={message}
            /> :
            null
        }
      </DescriptionList>
    );
  }

  if (eventType === 'downloadFolderImported') {
    const {
      preferredWordScore,
      droppedPath,
      importedPath
    } = data;

    return (
      <DescriptionList>
        <DescriptionListItem
          descriptionClassName={styles.description}
          title="Name"
          data={sourceTitle}
        />

        {
          droppedPath ?
            <DescriptionListItem
              descriptionClassName={styles.description}
              title="Source"
              data={droppedPath}
            /> :
            null
        }

        {
          importedPath ?
            <DescriptionListItem
              descriptionClassName={styles.description}
              title="Imported To"
              data={importedPath}
            /> :
            null
        }

        {
          preferredWordScore && preferredWordScore !== '0' ?
            <DescriptionListItem
              title="Preferred Word Score"
              data={formatPreferredWordScore(preferredWordScore)}
            /> :
            null
        }
      </DescriptionList>
    );
  }

  if (eventType === 'episodeFileDeleted') {
    const {
      reason,
      preferredWordScore
    } = data;

    let reasonMessage = '';

    switch (reason) {
      case 'Manual':
        reasonMessage = 'File was deleted by via UI';
        break;
      case 'MissingFromDisk':
        reasonMessage = 'Sonarr was unable to find the file on disk so the file was unlinked from the episode in the database';
        break;
      case 'Upgrade':
        reasonMessage = 'File was deleted to import an upgrade';
        break;
      default:
        reasonMessage = '';
    }

    return (
      <DescriptionList>
        <DescriptionListItem
          title="Name"
          data={sourceTitle}
        />

        <DescriptionListItem
          title="Reason"
          data={reasonMessage}
        />

        {
          preferredWordScore && preferredWordScore !== '0' ?
            <DescriptionListItem
              title="Preferred Word Score"
              data={formatPreferredWordScore(preferredWordScore)}
            /> :
            null
        }
      </DescriptionList>
    );
  }

  if (eventType === 'episodeFileRenamed') {
    const {
      sourcePath,
      sourceRelativePath,
      path,
      relativePath
    } = data;

    return (
      <DescriptionList>
        <DescriptionListItem
          title="Source Path"
          data={sourcePath}
        />

        <DescriptionListItem
          title="Source Relative Path"
          data={sourceRelativePath}
        />

        <DescriptionListItem
          title="Destination Path"
          data={path}
        />

        <DescriptionListItem
          title="Destination Relative Path"
          data={relativePath}
        />
      </DescriptionList>
    );
  }

  if (eventType === 'downloadIgnored') {
    const {
      message
    } = data;

    return (
      <DescriptionList>
        <DescriptionListItem
          descriptionClassName={styles.description}
          title="Name"
          data={sourceTitle}
        />

        {
          message ?
            <DescriptionListItem
              title="Message"
              data={message}
            /> :
            null
        }
      </DescriptionList>
    );
  }

  return (
    <DescriptionList>
      <DescriptionListItem
        descriptionClassName={styles.description}
        title="Name"
        data={sourceTitle}
      />
    </DescriptionList>
  );
}

HistoryDetails.propTypes = {
  eventType: PropTypes.string.isRequired,
  sourceTitle: PropTypes.string.isRequired,
  data: PropTypes.object.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired
};

export default HistoryDetails;
