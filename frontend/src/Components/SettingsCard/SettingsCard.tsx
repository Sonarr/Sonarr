import React, { ReactNode } from 'react';
import Card from 'Components/Card';
import styles from './SettingsCard.css';

interface SettingsCardProps {
  name: ReactNode;
  isUnnamed?: boolean;
  actions?: ReactNode;
  title?: string;
  'aria-label': string;
  onPress: () => void;
  children?: ReactNode;
}

function SettingsCard({
  name,
  isUnnamed = false,
  actions,
  title,
  'aria-label': ariaLabel,
  onPress,
  children,
}: SettingsCardProps) {
  return (
    <Card
      className={styles.card}
      overlayClassName={styles.overlay}
      overlayContent={true}
      title={title}
      aria-label={ariaLabel}
      onPress={onPress}
    >
      <div className={styles.nameContainer}>
        <div className={isUnnamed ? styles.unnamedName : styles.name}>
          {name}
        </div>

        {actions ? <div className={styles.rightCluster}>{actions}</div> : null}
      </div>

      {children}
    </Card>
  );
}

export default SettingsCard;
