import PropTypes from 'prop-types';
import React from 'react';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';

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
        {titleCase(translate('DelayProfileProtocol', { preferredProtocol }))}
      </div>

      <div>
        {
          enableUsenet ?
            translate('UsenetDelayTime', { usenetDelay }) :
            translate('UsenetDisabled')
        }
      </div>

      <div>
        {
          enableTorrent ?
            translate('TorrentDelayTime', { torrentDelay }) :
            translate('TorrentsDisabled')
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
