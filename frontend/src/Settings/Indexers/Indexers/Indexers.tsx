import React, { useCallback, useState } from 'react';
import PageSectionContent from 'Components/Page/PageSectionContent';
import AddCard from 'Components/SettingsCard/AddCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.css';
import { SelectedSchema } from 'Settings/useProviderSchema';
import translate from 'Utilities/String/translate';
import { useSortedIndexers } from '../useIndexers';
import AddIndexerModal from './AddIndexerModal';
import EditIndexerModal from './EditIndexerModal';
import Indexer from './Indexer';

function Indexers() {
  const { isFetching, isFetched, data, error } = useSortedIndexers();

  const [isAddIndexerModalOpen, setIsAddIndexerModalOpen] = useState(false);
  const [isEditIndexerModalOpen, setIsEditIndexerModalOpen] = useState(false);
  const [cloneIndexerId, setCloneIndexerId] = useState<number | null>(null);

  const showPriority = data.some((index) => index.priority !== 25);

  const [selectedSchema, setSelectedSchema] = useState<
    SelectedSchema | undefined
  >(undefined);

  const handleAddIndexerPress = useCallback(() => {
    setCloneIndexerId(null);
    setIsAddIndexerModalOpen(true);
  }, []);

  const handleCloneIndexerPress = useCallback((id: number) => {
    setCloneIndexerId(id);
    setIsEditIndexerModalOpen(true);
  }, []);

  const handleIndexerSelect = useCallback((selected: SelectedSchema) => {
    setSelectedSchema(selected);
    setIsAddIndexerModalOpen(false);
    setIsEditIndexerModalOpen(true);
  }, []);

  const handleAddIndexerModalClose = useCallback(() => {
    setIsAddIndexerModalOpen(false);
  }, []);

  const handleEditIndexerModalClose = useCallback(() => {
    setCloneIndexerId(null);
    setIsEditIndexerModalOpen(false);
  }, []);

  return (
    <PageSectionContent
      errorMessage={translate('IndexersLoadError')}
      error={error}
      isFetching={isFetching}
      isPopulated={isFetched}
    >
      <div className={settingsCardStyles.grid}>
        {data.map((item) => {
          return (
            <Indexer
              key={item.id}
              {...item}
              showPriority={showPriority}
              onCloneIndexerPress={handleCloneIndexerPress}
            />
          );
        })}

        <AddCard
          label={translate('AddIndexer')}
          onPress={handleAddIndexerPress}
        />
      </div>

      <AddIndexerModal
        isOpen={isAddIndexerModalOpen}
        onIndexerSelect={handleIndexerSelect}
        onModalClose={handleAddIndexerModalClose}
      />

      <EditIndexerModal
        isOpen={isEditIndexerModalOpen}
        cloneId={cloneIndexerId ?? undefined}
        selectedSchema={selectedSchema}
        onModalClose={handleEditIndexerModalClose}
      />
    </PageSectionContent>
  );
}

export default Indexers;
