import React from 'react';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import styles from './UpdateChanges.css';

interface UpdateChangesProps {
  title: string;
  changes: string[];
}

function UpdateChanges(props: UpdateChangesProps) {
  const { title, changes } = props;

  if (changes.length === 0) {
    return null;
  }

  return (
    <div>
      <div className={styles.title}>{title}</div>
      <ul>
        {changes.map((change, index) => {
          return (
            <li key={index}>
              <InlineMarkdown data={change} />
            </li>
          );
        })}
      </ul>
    </div>
  );
}

export default UpdateChanges;
