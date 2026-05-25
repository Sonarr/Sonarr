import React, { type ReactElement } from 'react';
import { InternalOverflowItem } from './Overflow';
import styles from './ToolbarItem.css';

interface ToolbarItemProps {
  id: string;
  priority?: number;
  pinned?: boolean;
  groupId?: string;
  children: ReactElement;
}

// Slot div: inter-item spacing lives in border-box padding (Fluent's budget doesn't count margin/gap).
function ToolbarItem({
  id,
  priority,
  pinned,
  groupId,
  children,
}: ToolbarItemProps) {
  return (
    <InternalOverflowItem
      id={id}
      priority={priority}
      pinned={pinned}
      groupId={groupId}
    >
      <div className={styles.slot}>{children}</div>
    </InternalOverflowItem>
  );
}

export default ToolbarItem;
