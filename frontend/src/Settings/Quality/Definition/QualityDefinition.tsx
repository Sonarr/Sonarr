import React, { useCallback } from 'react';
import TextInput from 'Components/Form/TextInput';
import Quality from 'Quality/Quality';
import { useManageQualityDefinitions } from './useQualityDefinitions';
import styles from './QualityDefinition.css';

interface QualityDefinitionProps {
  id: number;
  quality: Quality;
  title: string;
  updateDefinition: ReturnType<
    typeof useManageQualityDefinitions
  >['updateDefinition'];
}

function QualityDefinition({
  id,
  quality,
  title,
  updateDefinition,
}: QualityDefinitionProps) {
  const handleTitleChange = useCallback(
    ({ value }: { value: string }) => {
      updateDefinition(id, 'title', value);
    },
    [id, updateDefinition]
  );

  return (
    <div className={styles.qualityDefinition}>
      <div className={styles.quality}>{quality.name}</div>

      <div className={styles.title}>
        <TextInput
          name={`${id}.${title}`}
          value={title}
          onChange={handleTitleChange}
        />
      </div>
    </div>
  );
}

export default QualityDefinition;
