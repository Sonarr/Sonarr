import _ from 'lodash';
import PropTypes from 'prop-types';
import React from 'react';
import * as mediaInfoTypes from './mediaInfoTypes';

function formatLanguages(languages) {
  if (!languages) {
    return null;
  }

  const splitLanguages = _.uniq(languages.split(' / '));

  if (splitLanguages.length > 3) {
    return (
      <span title={splitLanguages.join(', ')}>
        {splitLanguages.slice(0, 2).join(', ')}, {splitLanguages.length - 2} more
      </span>
    );
  }

  return (
    <span>
      {splitLanguages.join(', ')}
    </span>
  );
}

function MediaInfo(props) {
  const {
    type,
    audioChannels,
    audioCodec,
    audioLanguages,
    subtitles,
    videoCodec,
    videoDynamicRangeType
  } = props;

  if (type === mediaInfoTypes.AUDIO) {
    return (
      <span>
        {
          !!audioCodec &&
            audioCodec
        }

        {
          !!audioCodec && !!audioChannels &&
          ' - '
        }

        {
          !!audioChannels &&
          audioChannels.toFixed(1)
        }
      </span>
    );
  }

  if (type === mediaInfoTypes.AUDIO_LANGUAGES) {
    return formatLanguages(audioLanguages);
  }

  if (type === mediaInfoTypes.SUBTITLES) {
    return formatLanguages(subtitles);
  }

  if (type === mediaInfoTypes.VIDEO) {
    return (
      <span>
        {videoCodec}
      </span>
    );
  }

  if (type === mediaInfoTypes.VIDEO_DYNAMIC_RANGE_TYPE) {
    return (
      <span>
        {videoDynamicRangeType}
      </span>
    );
  }

  return null;
}

MediaInfo.propTypes = {
  type: PropTypes.string.isRequired,
  audioChannels: PropTypes.number,
  audioCodec: PropTypes.string,
  audioLanguages: PropTypes.string,
  subtitles: PropTypes.string,
  videoCodec: PropTypes.string,
  videoDynamicRangeType: PropTypes.string
};

export default MediaInfo;
