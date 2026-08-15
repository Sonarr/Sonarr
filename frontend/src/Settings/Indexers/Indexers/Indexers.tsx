import React, { useCallback, useMemo, useState } from 'react';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import {
  getProviderTestStatus,
  ProviderTestAllResult,
} from 'Settings/ProviderTestAllResult';
import { SelectedSchema } from 'Settings/useProviderSchema';
import { ApiError } from 'Utilities/Fetch/fetchJson';
import translate from 'Utilities/String/translate';
import { useSortedIndexers } from '../useIndexers';
import AddIndexerModal from './AddIndexerModal';
import EditIndexerModal from './EditIndexerModal';
import Indexer from './Indexer';
import IndexerTestAllResults from './IndexerTestAllResults';
import styles from './Indexers.css';

interface IndexersProps {
  testResults?: ProviderTestAllResult[];
  testError?: ApiError | null;
}

function Indexers({ testResults, testError }: IndexersProps) {
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
    <FieldSet legend={translate('Indexers')}>
      <PageSectionContent
        errorMessage={translate('IndexersLoadError')}
        error={error}
        isFetching={isFetching}
        isPopulated={isFetched}
      >
        <IndexerTestAllResults
          indexers={data}
          results={testResults}
          error={testError}
        />

        <div className={styles.indexers}>
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

          <Card
            className={styles.addIndexer}
            aria-label={translate('AddIndexer')}
            onPress={handleAddIndexerPress}
          >
            <div className={styles.center}>
              <Icon name={icons.ADD} size={45} />
            </div>
          </Card>
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
    </FieldSet>
  );
}

export default Indexers;
