import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import MediaInfoProps from 'typings/MediaInfo';
import formatBitrate from 'Utilities/Number/formatBitrate';
import getEntries from 'Utilities/Object/getEntries';
import getLanguageName from 'Utilities/String/getLanguageName';
import translate from 'Utilities/String/translate';

function MediaInfo(props: MediaInfoProps) {
  return (
    <DescriptionList>
      {getEntries(props).map(([key, value]) => {
        if (!value) {
          return null;
        }

        const title = key
          .replace(/([A-Z])/g, ' $1')
          .replace(/^./, (str) => str.toUpperCase());

        if (key === 'audioStreams') {
          return value.map((audioStream, index) => {
            const language = getLanguageName(audioStream.language);

            let line = `${language}`;

            if (
              audioStream.title !== undefined &&
              audioStream.title !== language
            ) {
              line += ` / ${audioStream.title}`;
            }

            line += ` / ${audioStream.codec || translate('Unknown')}`;
            line += ` / ${audioStream.channels}ch`;
            line += ` / ${
              audioStream.channelPositions || translate('Unknown')
            }`;
            line += ` / ${formatBitrate(audioStream.bitrate)}`;

            return (
              <DescriptionListItem
                key={`audio-stream-${index}`}
                title={translate('MediaInfoAudioStreamHeader', {
                  number: index + 1,
                })}
                data={line}
              />
            );
          });
        }

        if (key === 'subtitleStreams') {
          return (
            <DescriptionListItem
              key={key}
              title={translate('MediaInfoSubtitlesHeader')}
              data={value.reduce(
                (acc: React.ReactNode[] | null, subtitleStream, index) => {
                  const language = getLanguageName(subtitleStream.language);

                  let line = `${
                    subtitleStream.format?.toUpperCase() || translate('Unknown')
                  }`;

                  if (
                    subtitleStream.title !== undefined &&
                    subtitleStream.title !== language
                  ) {
                    line += ` | ${subtitleStream.title}`;
                  }

                  if (subtitleStream.forced) {
                    line += ` | ${translate('MediaInfoForced')}`;
                  }

                  if (subtitleStream.hearingImpaired) {
                    line += ` | ${translate('MediaInfoHearingImpaired')}`;
                  }

                  const curr = (
                    <span key={index} title={line}>
                      {language}
                    </span>
                  );

                  return acc === null ? [curr] : [acc, ' / ', curr];
                },
                null
              )}
            />
          );
        }

        if (key === 'videoBitrate') {
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
