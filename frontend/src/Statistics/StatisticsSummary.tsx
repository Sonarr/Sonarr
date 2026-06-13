import React from 'react';
import styles from './StatisticsSummary.css';

export interface SummaryItem {
  label: string;
  value: string;
  secondary?: string;
}

interface StatisticsSummaryProps {
  items: SummaryItem[];
}

function StatisticsSummary({ items }: StatisticsSummaryProps) {
  return (
    <div className={styles.summary}>
      {items.map((item) => {
        return (
          <div key={item.label} className={styles.item}>
            <div className={styles.value}>{item.value}</div>
            <div className={styles.label}>{item.label}</div>

            {item.secondary ? (
              <div className={styles.secondary}>{item.secondary}</div>
            ) : null}
          </div>
        );
      })}
    </div>
  );
}

export default StatisticsSummary;
