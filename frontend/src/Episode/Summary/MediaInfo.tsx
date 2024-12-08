import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import MediaInfoProps from 'typings/MediaInfo';
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

        return (
          <DescriptionListItem key={key} title={title} data={props[key]} />
        );
      })}
    </DescriptionList>
  );
}

export default MediaInfo;
