import React from 'react';
import styles from './PageToolbarSpacer.css';

// Marker for PageToolbar: items before render left-aligned, items after
// right-aligned, and the ⋮ overflow button sits at this position.
function PageToolbarSpacer() {
  return <div className={styles.spacer} />;
}

export default PageToolbarSpacer;
