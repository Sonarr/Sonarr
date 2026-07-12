import React, { useCallback, useState } from 'react';
import Label from 'Components/Label';
import MiddleTruncate from 'Components/MiddleTruncate';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import SettingsCard from 'Components/SettingsCard/SettingsCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import SettingsCardAction from 'Components/SettingsCard/SettingsCardAction';
import SettingsCardStatus from 'Components/SettingsCard/SettingsCardStatus';
import { icons, kinds } from 'Helpers/Props';
import { Kind } from 'Helpers/Props/kinds';
import translate from 'Utilities/String/translate';
import EditCustomFormatModal from './EditCustomFormatModal';
import ExportCustomFormatModal from './ExportCustomFormatModal';
import {
  CustomFormatSpecification,
  useDeleteCustomFormat,
} from './useCustomFormats';

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

  const conditionCount = specifications.length;
  const isActive = conditionCount > 0;

  return (
    <SettingsCard
      name={name}
      aria-label={translate('EditCustomFormatName', { name })}
      actions={
        <>
          <SettingsCardAction
            title={translate('CloneCustomFormat')}
            aria-label={translate('CloneCustomFormat')}
            name={icons.CLONE}
            onPress={handleCloneCustomFormatPressHandler}
          />

          <SettingsCardAction
            title={translate('ExportCustomFormat')}
            aria-label={translate('ExportCustomFormat')}
            name={icons.EXPORT}
            onPress={handleExportCustomFormatPress}
          />
        </>
      }
      onPress={onEditCustomFormatPress}
    >
      <SettingsCardStatus
        dot={isActive ? 'active' : 'muted'}
        segments={[translate('ConditionsCount', { count: conditionCount })]}
      />

      {conditionCount ? (
        <div className={settingsCardStyles.labels}>
          {specifications.map((specification) => {
            if (!specification) {
              return null;
            }

            let kind: Kind = kinds.DEFAULT;

            if (specification.required) {
              kind = kinds.SUCCESS;
            }

            if (specification.negate) {
              kind = kinds.DANGER;
            }

            return (
              <Label
                key={specification.id}
                className={settingsCardStyles.truncatedLabel}
                kind={kind}
                dot={specification.required || specification.negate}
              >
                <MiddleTruncate text={specification.name} />
              </Label>
            );
          })}
        </div>
      ) : null}

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
    </SettingsCard>
  );
}

export default CustomFormat;
