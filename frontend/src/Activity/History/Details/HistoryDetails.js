import PropTypes from 'prop-types';
import React from 'react';
import formatDateTime from 'Utilities/Date/formatDateTime';
import formatAge from 'Utilities/Number/formatAge';
import Link from 'Components/Link/Link';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import DescriptionListItemTitle from 'Components/DescriptionList/DescriptionListItemTitle';
import DescriptionListItemDescription from 'Components/DescriptionList/DescriptionListItemDescription';

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
      nzbInfoUrl,
      downloadClient,
      downloadId,
      age,
      ageHours,
      ageMinutes,
      publishedDate
    } = data;

    return (
      <DescriptionList>
        <DescriptionListItem
          title="Name"
          data={sourceTitle}
        />

        {
          !!indexer &&
            <DescriptionListItem
              title="Indexer"
              data={indexer}
            />
        }

        {
          !!releaseGroup &&
            <DescriptionListItem
              title="Release Group"
              data={releaseGroup}
            />
        }

        {
          !!nzbInfoUrl &&
            <span>
              <DescriptionListItemTitle>
                Info URL
              </DescriptionListItemTitle>

              <DescriptionListItemDescription>
                <Link to={nzbInfoUrl}>{nzbInfoUrl}</Link>
              </DescriptionListItemDescription>
            </span>
        }

        {
          !!downloadClient &&
            <DescriptionListItem
              title="Download Client"
              data={downloadClient}
            />
        }

        {
          !!downloadId &&
            <DescriptionListItem
              title="Grab ID"
              data={downloadId}
            />
        }

        {
          !!indexer &&
            <DescriptionListItem
              title="Age (when grabbed)"
              data={formatAge(age, ageHours, ageMinutes)}
            />
        }

        {
          !!publishedDate &&
            <DescriptionListItem
              title="Published Date"
              data={formatDateTime(publishedDate, shortDateFormat, timeFormat, { includeSeconds: true })}
            />
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
          title="Name"
          data={sourceTitle}
        />

        {
          !!message &&
            <DescriptionListItem
              title="Message"
              data={message}
            />
        }
      </DescriptionList>
    );
  }

  if (eventType === 'downloadFolderImported') {
    const {
      droppedPath,
      importedPath
    } = data;

    return (
      <DescriptionList>
        <DescriptionListItem
          title="Name"
          data={sourceTitle}
        />

        {
          !!droppedPath &&
            <DescriptionListItem
              title="Source"
              data={droppedPath}
            />
        }

        {
          !!importedPath &&
            <DescriptionListItem
              title="Imported To"
              data={importedPath}
            />
        }
      </DescriptionList>
    );
  }

  if (eventType === 'episodeFileDeleted') {
    const {
      reason
    } = data;

    let reasonMessage = '';

    switch (reason) {
      case 'Manual':
        reasonMessage = 'File was deleted by via UI';
        break;
      case 'MissingFromDisk':
        reasonMessage = 'Sonarr was unable to find the file on disk so it was removed';
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
}

HistoryDetails.propTypes = {
  eventType: PropTypes.string.isRequired,
  sourceTitle: PropTypes.string.isRequired,
  data: PropTypes.object.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired
};

export default HistoryDetails;
