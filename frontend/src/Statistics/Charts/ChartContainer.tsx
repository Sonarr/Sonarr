import React from 'react';
import styles from './ChartContainer.css';

interface ChartContainerProps {
  title: string;
  children: React.ReactNode;
}

function ChartContainer({ title, children }: ChartContainerProps) {
  return (
    <div className={styles.container}>
      <div className={styles.title}>{title}</div>

      <div className={styles.chart}>{children}</div>
    </div>
  );
}

export default ChartContainer;
