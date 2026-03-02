import React, { useCallback } from 'react';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import { sizes } from 'Helpers/Props';
import { SelectedSchema } from 'Settings/useProviderSchema';
import translate from 'Utilities/String/translate';
import styles from './AddExternalDecisionItem.css';

interface AddExternalDecisionItemProps {
  implementation: string;
  implementationName: string;
  infoLink: string;
  onExternalDecisionSelect: (selectedSchema: SelectedSchema) => void;
}

function AddExternalDecisionItem({
  implementation,
  implementationName,
  infoLink,
  onExternalDecisionSelect,
}: AddExternalDecisionItemProps) {
  const handleExternalDecisionSelect = useCallback(() => {
    onExternalDecisionSelect({ implementation, implementationName });
  }, [implementation, implementationName, onExternalDecisionSelect]);

  return (
    <div className={styles.externalDecision}>
      <Link
        className={styles.underlay}
        onPress={handleExternalDecisionSelect}
      />

      <div className={styles.overlay}>
        <div className={styles.name}>{implementationName}</div>

        <div className={styles.actions}>
          <Button to={infoLink} size={sizes.SMALL}>
            {translate('MoreInfo')}
          </Button>
        </div>
      </div>
    </div>
  );
}

export default AddExternalDecisionItem;
