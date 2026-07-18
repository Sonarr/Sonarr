import React, { ReactNode } from 'react';
import styles from './SectionHeading.css';

interface SectionHeadingProps {
  title: ReactNode;
  description?: ReactNode;
}

function SectionHeading({ title, description }: SectionHeadingProps) {
  return (
    <hgroup className={styles.sectionHeading}>
      <h2 className={styles.title}>{title}</h2>

      {description ? <p className={styles.description}>{description}</p> : null}
    </hgroup>
  );
}

export default SectionHeading;
