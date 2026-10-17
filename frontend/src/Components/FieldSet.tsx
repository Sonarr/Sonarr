import React, { ReactNode } from 'react';
import SectionHeading from './SectionHeading';
import styles from './FieldSet.module.css';

interface FieldSetProps {
  legend?: ReactNode;
  caption?: ReactNode;
  notice?: ReactNode;
  children?: ReactNode;
}

function FieldSet({ legend, caption, notice, children }: FieldSetProps) {
  return (
    <div className={styles.section}>
      {legend ? <SectionHeading title={legend} description={caption} /> : null}

      {notice ? <div className={styles.notice}>{notice}</div> : null}

      <section className={styles.fieldSet}>{children}</section>
    </div>
  );
}

export default FieldSet;
