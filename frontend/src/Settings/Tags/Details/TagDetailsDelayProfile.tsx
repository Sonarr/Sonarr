import React from 'react';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';

interface TagDetailsDelayProfileProps {
  preferredProtocol: string;
  enableUsenet: boolean;
  enableTorrent: boolean;
  usenetDelay: number;
  torrentDelay: number;
}

function TagDetailsDelayProfile({
  preferredProtocol,
  enableUsenet,
  enableTorrent,
  usenetDelay,
  torrentDelay,
}: TagDetailsDelayProfileProps) {
  const protocol = titleCase(
    translate('DelayProfileProtocol', {
      preferredProtocol:
        preferredProtocol === 'unknown'
          ? translate('NoPreference')
          : preferredProtocol,
    })
  );

  return (
    <div>
      <div>{protocol}</div>

      <div>
        {enableUsenet
          ? translate('UsenetDelayTime', { usenetDelay })
          : translate('UsenetDisabled')}
      </div>

      <div>
        {enableTorrent
          ? translate('TorrentDelayTime', { torrentDelay })
          : translate('TorrentsDisabled')}
      </div>
    </div>
  );
}

export default TagDetailsDelayProfile;
