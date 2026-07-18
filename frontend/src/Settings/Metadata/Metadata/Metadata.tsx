import React, { useCallback, useMemo, useState } from 'react';
import Label from 'Components/Label';
import SettingsCard from 'Components/SettingsCard/SettingsCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import SettingsCardStatus from 'Components/SettingsCard/SettingsCardStatus';
import { kinds, sizes } from 'Helpers/Props';
import Field from 'typings/Field';
import translate from 'Utilities/String/translate';
import EditMetadataModal from './EditMetadataModal';
import styles from './Metadata.css';

interface MetadataProps {
  id: number;
  name: string;
  enable: boolean;
  fields: Field[];
}

function Metadata({ id, name, enable, fields }: MetadataProps) {
  const [isEditMetadataModalOpen, setIsEditMetadataModalOpen] = useState(false);

  const { metadataFields, imageFields } = useMemo(() => {
    return fields.reduce<{ metadataFields: Field[]; imageFields: Field[] }>(
      (acc, field) => {
        if (field.section === 'metadata') {
          acc.metadataFields.push(field);
        } else {
          acc.imageFields.push(field);
        }

        return acc;
      },
      { metadataFields: [], imageFields: [] }
    );
  }, [fields]);

  const handleOpenPress = useCallback(() => {
    setIsEditMetadataModalOpen(true);
  }, []);

  const handleModalClose = useCallback(() => {
    setIsEditMetadataModalOpen(false);
  }, []);

  return (
    <SettingsCard
      name={name}
      aria-label={translate('MetadataName', { name })}
      onPress={handleOpenPress}
    >
      <SettingsCardStatus
        dot={enable ? 'active' : 'muted'}
        segments={[translate(enable ? 'Enabled' : 'Disabled')]}
      />

      {enable && metadataFields.length ? (
        <div className={styles.fieldSection}>
          <div className={styles.section}>{translate('Metadata')}</div>

          <div className={settingsCardStyles.labels}>
            {metadataFields.map((field) => {
              if (!field.value) {
                return null;
              }

              return (
                <Label
                  key={field.label}
                  dot={false}
                  kind={kinds.DEFAULT}
                  size={sizes.MEDIUM}
                >
                  {field.label}
                </Label>
              );
            })}
          </div>
        </div>
      ) : null}

      {enable && imageFields.length ? (
        <div className={styles.fieldSection}>
          <div className={styles.section}>{translate('Images')}</div>

          <div className={settingsCardStyles.labels}>
            {imageFields.map((field) => {
              if (!field.value) {
                return null;
              }

              return (
                <Label
                  key={field.label}
                  dot={false}
                  kind={kinds.DEFAULT}
                  size={sizes.MEDIUM}
                >
                  {field.label}
                </Label>
              );
            })}
          </div>
        </div>
      ) : null}

      <EditMetadataModal
        id={id}
        isOpen={isEditMetadataModalOpen}
        onModalClose={handleModalClose}
      />
    </SettingsCard>
  );
}

export default Metadata;
