import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import MediaInfoProps from 'typings/MediaInfo';
import formatBitrate from 'Utilities/Number/formatBitrate';
import getEntries from 'Utilities/Object/getEntries';
import styles from './MediaInfo.css';

function MediaInfo(props: MediaInfoProps) {
  return (
    <div className={styles.mediaInfo}>
      <div>
        <DescriptionList>
          {getEntries(props).map(([key, value]) => {
            if (key === 'audioStreams') {
              return null;
            }

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
      </div>
      <div className={styles.audioInfo}>
      {props.audioStreams && props.audioStreams.length > 0 && (
        (() => {
          const audioStreamsArray = props.audioStreams.split('+');
          return audioStreamsArray.map((audioStream, index) => {
            const title = audioStreamsArray.length === 1 ? 'Audio Stream' : `Audio Stream #${index + 1}`;
            return <DescriptionListItem key={`audioStream-${index}`} title={title} data={audioStream} />;
          });
        })()
      )}
      </div>
    </div>
  );
}

export default MediaInfo;
