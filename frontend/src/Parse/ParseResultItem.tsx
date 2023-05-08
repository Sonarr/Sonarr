import React, { ReactNode } from 'react';
import styles from './ParseResultItem.css';

interface ParseResultItemProps {
  title: string;
  data: string | number | ReactNode;
}

function ParseResultItem(props: ParseResultItemProps) {
  const { title, data } = props;

  return (
    <div className={styles.item}>
      <div className={styles.title}>{title}</div>
      <div>{data}</div>
    </div>
  );
}

export default ParseResultItem;
