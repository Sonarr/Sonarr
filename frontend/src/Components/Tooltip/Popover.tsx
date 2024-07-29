import React from 'react';
import Tooltip, { TooltipProps } from './Tooltip';
import styles from './Popover.css';

interface PopoverProps extends Omit<TooltipProps, 'tooltip' | 'bodyClassName'> {
  title: string;
  body: React.ReactNode;
}

function Popover({ title, body, ...otherProps }: PopoverProps) {
  return (
    <Tooltip
      {...otherProps}
      bodyClassName={styles.tooltipBody}
      tooltip={
        <div>
          <div className={styles.title}>{title}</div>

          <div className={styles.body}>{body}</div>
        </div>
      }
    />
  );
}

export default Popover;
