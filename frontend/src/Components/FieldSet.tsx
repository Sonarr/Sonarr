import React, { ReactNode } from 'react';
import SectionHeading from './SectionHeading';
import styles from './FieldSet.css';

interface FieldSetProps {
  legend?: ReactNode;
  caption?: ReactNode;
  children?: ReactNode;
}

function FieldSet({ legend, caption, children }: FieldSetProps) {
  return (
    <div className={styles.section}>
      {legend ? <SectionHeading title={legend} description={caption} /> : null}

      <section className={styles.fieldSet}>{children}</section>
    </div>
  );
}

export default FieldSet;
