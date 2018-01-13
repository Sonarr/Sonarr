import PropTypes from 'prop-types';
import React from 'react';
import titleCase from 'Utilities/String/titleCase';

function TagDetailsDelayProfile(props) {
  const {
    preferredProtocol,
    enableUsenet,
    enableTorrent,
    usenetDelay,
    torrentDelay
  } = props;

  return (
    <div>
      <div>
        Protocol: {titleCase(preferredProtocol)}
      </div>

      <div>
        {
          enableUsenet ?
            `Usenet Delay: ${usenetDelay}` :
            'Usenet disabled'
        }
      </div>

      <div>
        {
          enableTorrent ?
            `Torrent Delay: ${torrentDelay}` :
            'Torrents disabled'
        }
      </div>
    </div>
  );
}

TagDetailsDelayProfile.propTypes = {
  preferredProtocol: PropTypes.string.isRequired,
  enableUsenet: PropTypes.bool.isRequired,
  enableTorrent: PropTypes.bool.isRequired,
  usenetDelay: PropTypes.number.isRequired,
  torrentDelay: PropTypes.number.isRequired
};

export default TagDetailsDelayProfile;
