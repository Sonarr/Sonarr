import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';

function MediaInfo(props) {
  return (
    <DescriptionList>
      {
        Object.keys(props).map((key) => {
          const title = key
            .replace(/([A-Z])/g, ' $1')
            .replace(/^./, (str) => str.toUpperCase());

          const value = props[key];

          if (!value) {
            return null;
          }

          return (
            <DescriptionListItem
              key={key}
              title={title}
              data={props[key]}
            />
          );
        })
      }
    </DescriptionList>
  );
}

export default MediaInfo;
