import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';

function getKind(seeders: number = 0) {
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

function getPeersTooltipPart(peersUnit: string, peers?: number) {
  if (peers == null) {
    return `Unknown ${peersUnit}s`;
  }

  if (peers === 1) {
    return `1 ${peersUnit}`;
  }

  return `${peers} ${peersUnit}s`;
}

interface PeersProps {
  seeders?: number;
  leechers?: number;
}

function Peers(props: PeersProps) {
  const { seeders, leechers } = props;

  const kind = getKind(seeders);

  return (
    <Label
      kind={kind}
      title={`${getPeersTooltipPart('seeder', seeders)}, ${getPeersTooltipPart(
        'leecher',
        leechers
      )}`}
    >
      {seeders == null ? '-' : seeders} / {leechers == null ? '-' : leechers}
    </Label>
  );
}

export default Peers;
