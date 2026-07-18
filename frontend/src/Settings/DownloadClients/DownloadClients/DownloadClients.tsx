import React, { useCallback, useState } from 'react';
import PageSectionContent from 'Components/Page/PageSectionContent';
import AddCard from 'Components/SettingsCard/AddCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import { SelectedSchema } from 'Settings/useProviderSchema';
import translate from 'Utilities/String/translate';
import AddDownloadClientModal from './AddDownloadClientModal';
import DownloadClient from './DownloadClient';
import EditDownloadClientModal from './EditDownloadClientModal';
import { useSortedDownloadClients } from './useDownloadClients';

function DownloadClients() {
  const { isFetching, isFetched, data, error } = useSortedDownloadClients();

  const [isAddDownloadClientModalOpen, setIsAddDownloadClientModalOpen] =
    useState(false);
  const [isEditDownloadClientModalOpen, setIsEditDownloadClientModalOpen] =
    useState(false);
  const [cloneDownloadClientId, setCloneDownloadClientId] = useState<
    number | null
  >(null);
  const [selectedSchema, setSelectedSchema] = useState<
    SelectedSchema | undefined
  >(undefined);

  const handleAddDownloadClientPress = useCallback(() => {
    setCloneDownloadClientId(null);
    setIsAddDownloadClientModalOpen(true);
  }, []);

  const handleCloneDownloadClientPress = useCallback((id: number) => {
    setCloneDownloadClientId(id);
    setIsEditDownloadClientModalOpen(true);
  }, []);

  const handleDownloadClientSelect = useCallback((selected: SelectedSchema) => {
    setSelectedSchema(selected);
    setIsAddDownloadClientModalOpen(false);
    setIsEditDownloadClientModalOpen(true);
  }, []);

  const handleAddDownloadClientModalClose = useCallback(() => {
    setIsAddDownloadClientModalOpen(false);
  }, []);

  const handleEditDownloadClientModalClose = useCallback(() => {
    setCloneDownloadClientId(null);
    setIsEditDownloadClientModalOpen(false);
  }, []);

  return (
    <PageSectionContent
      errorMessage={translate('DownloadClientsLoadError')}
      error={error}
      isFetching={isFetching}
      isPopulated={isFetched}
    >
      <div className={settingsCardStyles.grid}>
        {data.map((item) => {
          return (
            <DownloadClient
              key={item.id}
              {...item}
              onCloneDownloadClientPress={handleCloneDownloadClientPress}
            />
          );
        })}

        <AddCard
          label={translate('AddDownloadClient')}
          onPress={handleAddDownloadClientPress}
        />
      </div>

      <AddDownloadClientModal
        isOpen={isAddDownloadClientModalOpen}
        onDownloadClientSelect={handleDownloadClientSelect}
        onModalClose={handleAddDownloadClientModalClose}
      />

      <EditDownloadClientModal
        isOpen={isEditDownloadClientModalOpen}
        cloneId={cloneDownloadClientId ?? undefined}
        selectedSchema={selectedSchema}
        onModalClose={handleEditDownloadClientModalClose}
      />
    </PageSectionContent>
  );
}

export default DownloadClients;
