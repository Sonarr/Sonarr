import React from 'react';
import styles from './PageHeading.css';

interface PageHeadingProps {
  scope: string;
  title: string;
  meta?: React.ReactNode | string[];
  subtitle?: React.ReactNode;
  actions?: React.ReactNode;
}

function renderMeta(meta: PageHeadingProps['meta']) {
  if (Array.isArray(meta)) {
    return (
      <span className={styles.meta}>
        {meta.map((token, i) => (
          <React.Fragment key={i}>
            {i > 0 ? <span className={styles.metaSep}>·</span> : null}
            <span>{token}</span>
          </React.Fragment>
        ))}
      </span>
    );
  }

  return <span className={styles.meta}>{meta}</span>;
}

function PageHeading({
  scope,
  title,
  meta,
  subtitle,
  actions,
}: PageHeadingProps) {
  return (
    <div className={styles.heading}>
      <div className={styles.scopeLine}>
        <span className={styles.scope}>{scope}</span>
      </div>
      <div className={styles.row}>
        <div className={styles.titleBlock}>
          <h1 className={styles.title}>{title}</h1>
          {subtitle ? <p className={styles.subtitle}>{subtitle}</p> : null}
        </div>
        <div className={styles.right}>
          {meta ? renderMeta(meta) : null}
          {actions ? <div className={styles.actions}>{actions}</div> : null}
        </div>
      </div>
    </div>
  );
}

export default PageHeading;
