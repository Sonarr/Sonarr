import React, { useCallback } from 'react';
import Link from 'Components/Link/Link';
import styles from './PageJumpBarItem.css';

export interface PageJumpBarItemProps {
  label: string;
  onItemPress: (label: string) => void;
}

function PageJumpBarItem({ label, onItemPress }: PageJumpBarItemProps) {
  const handlePress = useCallback(() => {
    onItemPress(label);
  }, [label, onItemPress]);

  return (
    <Link className={styles.jumpBarItem} onPress={handlePress}>
      {label.toUpperCase()}
    </Link>
  );
}

export default PageJumpBarItem;
