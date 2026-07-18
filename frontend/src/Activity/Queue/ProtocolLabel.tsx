import React from 'react';
import Label, { LabelProps } from 'Components/Label';
import DownloadProtocol from 'DownloadClient/DownloadProtocol';
import styles from './ProtocolLabel.css';

interface ProtocolLabelProps
  extends Omit<LabelProps, 'children' | 'className'> {
  protocol: DownloadProtocol;
}

function ProtocolLabel({ protocol, ...otherProps }: ProtocolLabelProps) {
  const protocolName = protocol === 'usenet' ? 'nzb' : protocol;

  return (
    <Label className={styles[protocol]} {...otherProps}>
      {protocolName}
    </Label>
  );
}

export default ProtocolLabel;
