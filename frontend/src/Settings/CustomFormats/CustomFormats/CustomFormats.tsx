import React, { useCallback, useState } from 'react';
import PageSectionContent from 'Components/Page/PageSectionContent';
import AddCard from 'Components/SettingsCard/AddCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import translate from 'Utilities/String/translate';
import CustomFormat from './CustomFormat';
import EditCustomFormatModal from './EditCustomFormatModal';
import { useSortedCustomFormats } from './useCustomFormats';

function CustomFormats() {
  const {
    data: items,
    error,
    isFetching,
    isFetched: isPopulated,
  } = useSortedCustomFormats();

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [cloneId, setCloneId] = useState<number>();

  const handleAddCustomFormatPress = useCallback(() => {
    setCloneId(undefined);
    setIsEditModalOpen(true);
  }, []);

  const handleCloneCustomFormatPress = useCallback((id: number) => {
    setCloneId(id);
    setIsEditModalOpen(true);
  }, []);

  const handleEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
    setCloneId(undefined);
  }, []);

  return (
    <PageSectionContent
      errorMessage={translate('CustomFormatsLoadError')}
      isFetching={isFetching}
      isPopulated={isPopulated}
      error={error}
    >
      <div className={settingsCardStyles.grid}>
        {items.map((item) => {
          return (
            <CustomFormat
              key={item.id}
              {...item}
              onCloneCustomFormatPress={handleCloneCustomFormatPress}
            />
          );
        })}

        <AddCard
          label={translate('AddCustomFormat')}
          onPress={handleAddCustomFormatPress}
        />
      </div>

      <EditCustomFormatModal
        isOpen={isEditModalOpen}
        cloneId={cloneId}
        onModalClose={handleEditModalClose}
      />
    </PageSectionContent>
  );
}

export default CustomFormats;
