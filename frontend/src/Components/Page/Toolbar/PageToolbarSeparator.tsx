import React, { forwardRef } from 'react';
import styles from './PageToolbarSeparator.css';

const PageToolbarSeparator = forwardRef<HTMLDivElement>((_props, ref) => {
  return <div ref={ref} className={styles.separator} />;
});

export default PageToolbarSeparator;
