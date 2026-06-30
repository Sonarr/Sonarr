import React, { useCallback, useState } from 'react';
import Card from 'Components/Card';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { icons, kinds } from 'Helpers/Props';
import { Kind } from 'Helpers/Props/kinds';
import translate from 'Utilities/String/translate';
import EditCustomFormatModal from './EditCustomFormatModal';
import ExportCustomFormatModal from './ExportCustomFormatModal';
import {
  CustomFormatSpecification,
  useDeleteCustomFormat,
} from './useCustomFormats';
import styles from './CustomFormat.css';

interface CustomFormatProps {
  id: number;
  name: string;
  specifications: CustomFormatSpecification[];
  onCloneCustomFormatPress: (id: number) => void;
}

function CustomFormat({
  id,
  name,
  specifications,
  onCloneCustomFormatPress,
}: CustomFormatProps) {
  const { deleteCustomFormat, isDeleting } = useDeleteCustomFormat(id);

  const [isEditCustomFormatModalOpen, setIsEditCustomFormatModalOpen] =
    useState(false);
  const [isExportCustomFormatModalOpen, setIsExportCustomFormatModalOpen] =
    useState(false);
  const [isDeleteCustomFormatModalOpen, setIsDeleteCustomFormatModalOpen] =
    useState(false);

  const onEditCustomFormatPress = useCallback(() => {
    setIsEditCustomFormatModalOpen(true);
  }, []);

  const handleEditCustomFormatModalClose = useCallback(() => {
    setIsEditCustomFormatModalOpen(false);
  }, []);

  const handleDeleteCustomFormatPress = useCallback(() => {
    setIsEditCustomFormatModalOpen(false);
    setIsDeleteCustomFormatModalOpen(true);
  }, []);

  const handleDeleteCustomFormatModalClose = useCallback(() => {
    setIsDeleteCustomFormatModalOpen(false);
  }, []);

  const handleConfirmDeleteCustomFormat = useCallback(() => {
    deleteCustomFormat();
  }, [deleteCustomFormat]);

  const handleCloneCustomFormatPressHandler = useCallback(() => {
    onCloneCustomFormatPress(id);
  }, [id, onCloneCustomFormatPress]);

  const handleExportCustomFormatPress = useCallback(() => {
    setIsExportCustomFormatModalOpen(true);
  }, []);

  const handleExportCustomFormatModalClose = useCallback(() => {
    setIsExportCustomFormatModalOpen(false);
  }, []);

  return (
    <Card
      className={styles.customFormat}
      overlayContent={true}
      onPress={onEditCustomFormatPress}
    >
      <div className={styles.nameContainer}>
        <div className={styles.name}>{name}</div>

        <div className={styles.buttons}>
          <IconButton
            className={styles.cloneButton}
            title={translate('CloneCustomFormat')}
            aria-label={translate('CloneCustomFormat')}
            name={icons.CLONE}
            onPress={handleCloneCustomFormatPressHandler}
          />

          <IconButton
            className={styles.cloneButton}
            title={translate('ExportCustomFormat')}
            aria-label={translate('ExportCustomFormat')}
            name={icons.EXPORT}
            onPress={handleExportCustomFormatPress}
          />
        </div>
      </div>

      <div>
        {specifications.map((item, index) => {
          if (!item) {
            return null;
          }

          let kind: Kind = kinds.DEFAULT;

          if (item.required) {
            kind = kinds.SUCCESS;
          }

          if (item.negate) {
            kind = kinds.DANGER;
          }

          return (
            <Label key={index} className={styles.label} kind={kind}>
              {item.name}
            </Label>
          );
        })}
      </div>

      <EditCustomFormatModal
        id={id}
        isOpen={isEditCustomFormatModalOpen}
        onModalClose={handleEditCustomFormatModalClose}
        onDeleteCustomFormatPress={handleDeleteCustomFormatPress}
      />

      <ExportCustomFormatModal
        id={id}
        isOpen={isExportCustomFormatModalOpen}
        onModalClose={handleExportCustomFormatModalClose}
      />

      <ConfirmModal
        isOpen={isDeleteCustomFormatModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteCustomFormat')}
        message={translate('DeleteCustomFormatMessageText', { name })}
        confirmLabel={translate('Delete')}
        isSpinning={isDeleting}
        onConfirm={handleConfirmDeleteCustomFormat}
        onCancel={handleDeleteCustomFormatModalClose}
      />
    </Card>
  );
}

export default CustomFormat;
