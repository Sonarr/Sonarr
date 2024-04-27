import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import TextInput from 'Components/Form/TextInput';
import Quality from 'Quality/Quality';
import { setQualityDefinitionValue } from 'Store/Actions/settingsActions';
import styles from './QualityDefinition.css';

interface QualityDefinitionProps {
  id: number;
  quality: Quality;
  title: string;
}

function QualityDefinition(props: QualityDefinitionProps) {
  const { id, quality, title } = props;
  const dispatch = useDispatch();

  const handleTitleChange = useCallback(
    ({ value }: { value: string }) => {
      dispatch(
        setQualityDefinitionValue({
          id,
          name: 'title',
          value,
        })
      );
    },
    [id, dispatch]
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
