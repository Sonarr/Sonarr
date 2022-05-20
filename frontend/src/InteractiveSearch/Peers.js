import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';

function getKind(seeders) {
  if (seeders > 50) {
    return kinds.PRIMARY;
  }

  if (seeders > 10) {
    return kinds.INFO;
  }

  if (seeders > 0) {
    return kinds.WARNING;
  }

  return kinds.DANGER;
}

function getPeersTooltipPart(peers, peersUnit) {
  if (peers == null) {
    return `Unknown ${peersUnit}s`;
  }

  if (peers === 1) {
    return `1 ${peersUnit}`;
  }

  return `${peers} ${peersUnit}s`;
}

function Peers(props) {
  const {
    seeders,
    leechers
  } = props;

  const kind = getKind(seeders);

  return (
    <Label
      kind={kind}
      title={`${getPeersTooltipPart(seeders, 'seeder')}, ${getPeersTooltipPart(leechers, 'leecher')}`}
    >
      {seeders == null ? '-' : seeders} / {leechers == null ? '-' : leechers}
    </Label>
  );
}

Peers.propTypes = {
  seeders: PropTypes.number,
  leechers: PropTypes.number
};

export default Peers;
