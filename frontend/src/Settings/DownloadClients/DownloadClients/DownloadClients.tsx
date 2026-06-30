import React, { useCallback, useState } from 'react';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import { SelectedSchema } from 'Settings/useProviderSchema';
import translate from 'Utilities/String/translate';
import AddDownloadClientModal from './AddDownloadClientModal';
import DownloadClient from './DownloadClient';
import EditDownloadClientModal from './EditDownloadClientModal';
import { useSortedDownloadClients } from './useDownloadClients';
import styles from './DownloadClients.css';

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
    <FieldSet legend={translate('DownloadClients')}>
      <PageSectionContent
        errorMessage={translate('DownloadClientsLoadError')}
        error={error}
        isFetching={isFetching}
        isPopulated={isFetched}
      >
        <div className={styles.downloadClients}>
          {data.map((item) => {
            return (
              <DownloadClient
                key={item.id}
                {...item}
                onCloneDownloadClientPress={handleCloneDownloadClientPress}
              />
            );
          })}

          <Card
            className={styles.addDownloadClient}
            onPress={handleAddDownloadClientPress}
          >
            <div className={styles.center}>
              <Icon name={icons.ADD} size={45} />
            </div>
          </Card>
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
    </FieldSet>
  );
}

export default DownloadClients;
