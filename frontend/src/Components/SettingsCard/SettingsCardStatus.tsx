import React, { Fragment, ReactNode } from 'react';
import styles from './SettingsCard.css';

interface SettingsCardStatusProps {
  dot?: 'active' | 'muted';
  segments: ReactNode[];
}

export function SettingsCardStatusValue({ children }: { children: ReactNode }) {
  return <span className={styles.statusValue}>{children}</span>;
}

function SettingsCardStatus({ dot, segments }: SettingsCardStatusProps) {
  const visibleSegments = segments.filter(Boolean);

  if (!visibleSegments.length) {
    return null;
  }

  return (
    <div className={styles.statusLine}>
      {dot ? (
        <span
          className={
            dot === 'active' ? styles.statusDot : styles.statusDotMuted
          }
        />
      ) : null}

      {visibleSegments.map((segment, index) => (
        <Fragment key={index}>
          {index > 0 ? <span className={styles.statusSeparator}>·</span> : null}

          <span>{segment}</span>
        </Fragment>
      ))}
    </div>
  );
}

export default SettingsCardStatus;
