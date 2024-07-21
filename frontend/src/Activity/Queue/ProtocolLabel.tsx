import React from 'react';
import Label from 'Components/Label';
import DownloadProtocol from 'DownloadClient/DownloadProtocol';
import styles from './ProtocolLabel.css';

interface ProtocolLabelProps {
  protocol: DownloadProtocol;
}

function ProtocolLabel({ protocol }: ProtocolLabelProps) {
  const protocolName = protocol === 'usenet' ? 'nzb' : protocol;

  return <Label className={styles[protocol]}>{protocolName}</Label>;
}

export default ProtocolLabel;
