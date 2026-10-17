import React, { useCallback, useMemo, useState } from 'react';
import PageSectionContent from 'Components/Page/PageSectionContent';
import AddCard from 'Components/SettingsCard/AddCard';
import settingsCardStyles from 'Components/SettingsCard/SettingsCard.module.css';
import {
  getProviderTestStatus,
  ProviderTestAllResult,
} from 'Settings/ProviderTestAllResult';
import { SelectedSchema } from 'Settings/useProviderSchema';
import translate from 'Utilities/String/translate';
import { useSortedIndexers } from '../useIndexers';
import AddIndexerModal from './AddIndexerModal';
import EditIndexerModal from './EditIndexerModal';
import Indexer from './Indexer';

interface IndexersProps {
  testResults?: ProviderTestAllResult[];
}

function Indexers({ testResults }: IndexersProps) {
  const { isFetching, isFetched, data, error } = useSortedIndexers();

  const [isAddIndexerModalOpen, setIsAddIndexerModalOpen] = useState(false);
  const [isEditIndexerModalOpen, setIsEditIndexerModalOpen] = useState(false);
  const [cloneIndexerId, setCloneIndexerId] = useState<number | null>(null);

  const showPriority = data.some((index) => index.priority !== 25);
  const testResultById = useMemo(
    () => new Map(testResults?.map((result) => [result.id, result])),
    [testResults]
  );

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
              testStatus={
                testResults
                  ? getProviderTestStatus(testResultById.get(item.id))
                  : undefined
              }
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
