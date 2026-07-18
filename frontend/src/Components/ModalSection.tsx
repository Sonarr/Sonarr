import React, { ReactNode } from 'react';
import styles from './ModalSection.css';

interface ModalSectionProps {
  title: ReactNode;
  children: ReactNode;
}

function ModalSection({ title, children }: ModalSectionProps) {
  return (
    <section className={styles.section}>
      <h3 className={styles.heading}>{title}</h3>

      {children}
    </section>
  );
}

export default ModalSection;
