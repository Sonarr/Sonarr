import React, { useCallback, useMemo, useState } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
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
    <Card
      className={styles.metadata}
      overlayContent={true}
      onPress={handleOpenPress}
    >
      <div className={styles.name}>{name}</div>

      <div>
        {enable ? (
          <Label kind={kinds.SUCCESS}>{translate('Enabled')}</Label>
        ) : (
          <Label kind={kinds.DISABLED} outline={true}>
            {translate('Disabled')}
          </Label>
        )}
      </div>

      {enable && metadataFields.length ? (
        <div>
          <div className={styles.section}>{translate('Metadata')}</div>

          {metadataFields.map((field) => {
            if (!field.value) {
              return null;
            }

            return (
              <Label key={field.label} kind={kinds.SUCCESS}>
                {field.label}
              </Label>
            );
          })}
        </div>
      ) : null}

      {enable && imageFields.length ? (
        <div>
          <div className={styles.section}>{translate('Images')}</div>

          {imageFields.map((field) => {
            if (!field.value) {
              return null;
            }

            return (
              <Label key={field.label} kind={kinds.SUCCESS}>
                {field.label}
              </Label>
            );
          })}
        </div>
      ) : null}

      <EditMetadataModal
        id={id}
        isOpen={isEditMetadataModalOpen}
        onModalClose={handleModalClose}
      />
    </Card>
  );
}

export default Metadata;
