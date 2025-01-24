import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import MediaInfoProps from 'typings/MediaInfo';
import formatBitrate from 'Utilities/Number/formatBitrate';
import getEntries from 'Utilities/Object/getEntries';

function MediaInfo(props: MediaInfoProps) {
  return (
    <DescriptionList>
      {getEntries(props).map(([key, value]) => {
        const title = key
          .replace(/([A-Z])/g, ' $1')
          .replace(/^./, (str) => str.toUpperCase());

        if (!value) {
          return null;
        }

        if (key === 'audioBitrate' || key === 'videoBitrate') {
          return (
            <DescriptionListItem
              key={key}
              title={title}
              data={
                <span title={value.toString()}>{formatBitrate(value)}</span>
              }
            />
          );
        }

        return <DescriptionListItem key={key} title={title} data={value} />;
      })}
    </DescriptionList>
  );
}

export default MediaInfo;
