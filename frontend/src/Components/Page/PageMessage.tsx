import React, { ReactNode } from 'react';
import styles from './PageMessage.css';

interface PageMessageProps {
  children: ReactNode;
}

function PageMessage({ children }: PageMessageProps) {
  return <p className={styles.message}>{children}</p>;
}

export default PageMessage;
