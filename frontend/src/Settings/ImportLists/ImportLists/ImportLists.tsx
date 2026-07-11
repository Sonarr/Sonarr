import React, { useCallback, useState } from 'react';
import PageSectionContent from 'Components/Page/PageSectionContent';
import SectionHeading from 'Components/SectionHeading';
import AddCard from 'Components/SettingsCard/AddCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import settingsStyles from 'Settings/Settings.css';
import { SelectedSchema } from 'Settings/useProviderSchema';
import translate from 'Utilities/String/translate';
import AddImportListModal from './AddImportListModal';
import EditImportListModal from './EditImportListModal';
import ImportList from './ImportList';
import { useSortedImportLists } from './useImportLists';

function ImportLists() {
  const { isFetching, isFetched, data, error } = useSortedImportLists();

  const [isAddImportListModalOpen, setIsAddImportListModalOpen] =
    useState(false);
  const [isEditImportListModalOpen, setIsEditImportListModalOpen] =
    useState(false);
  const [cloneImportListId, setCloneImportListId] = useState<number | null>(
    null
  );

  const [selectedSchema, setSelectedSchema] = useState<
    SelectedSchema | undefined
  >(undefined);

  const handleAddImportListPress = useCallback(() => {
    setCloneImportListId(null);
    setIsAddImportListModalOpen(true);
  }, []);

  const handleAddImportListModalClose = useCallback(() => {
    setIsAddImportListModalOpen(false);
  }, []);

  const handleImportListSelect = useCallback((selected: SelectedSchema) => {
    setSelectedSchema(selected);
    setIsAddImportListModalOpen(false);
    setIsEditImportListModalOpen(true);
  }, []);

  const handleEditImportListModalClose = useCallback(() => {
    setCloneImportListId(null);
    setIsEditImportListModalOpen(false);
  }, []);

  const handleCloneImportListPress = useCallback((id: number) => {
    setCloneImportListId(id);
    setIsEditImportListModalOpen(true);
  }, []);

  return (
    <div className={settingsStyles.pageSection}>
      <SectionHeading
        title={translate('ImportLists')}
        description={translate('ImportListsSectionDescription')}
      />

      <PageSectionContent
        errorMessage={translate('ImportListsLoadError')}
        error={error}
        isFetching={isFetching}
        isPopulated={isFetched}
      >
        <div className={settingsCardStyles.grid}>
          {data.map((item) => {
            return (
              <ImportList
                key={item.id}
                {...item}
                onCloneImportListPress={handleCloneImportListPress}
              />
            );
          })}

          <AddCard
            label={translate('AddImportList')}
            onPress={handleAddImportListPress}
          />
        </div>

        <AddImportListModal
          isOpen={isAddImportListModalOpen}
          onImportListSelect={handleImportListSelect}
          onModalClose={handleAddImportListModalClose}
        />

        <EditImportListModal
          isOpen={isEditImportListModalOpen}
          cloneId={cloneImportListId ?? undefined}
          selectedSchema={selectedSchema}
          onModalClose={handleEditImportListModalClose}
        />
      </PageSectionContent>
    </div>
  );
}

export default ImportLists;
