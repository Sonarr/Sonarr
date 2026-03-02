import React, { useCallback, useState } from 'react';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import { SelectedSchema } from 'Settings/useProviderSchema';
import translate from 'Utilities/String/translate';
import {
  useExternalDecisions,
  useSortedExternalDecisions,
} from '../useExternalDecisions';
import AddExternalDecisionModal from './AddExternalDecisionModal';
import EditExternalDecisionModal from './EditExternalDecisionModal';
import ExternalDecision from './ExternalDecision';
import styles from './ExternalDecisions.css';

function ExternalDecisions() {
  const { error, isFetching, isFetched } = useExternalDecisions();
  const items = useSortedExternalDecisions();

  const showPriority = items.some((item) => item.priority !== 25);

  const [selectedSchema, setSelectedSchema] = useState<
    SelectedSchema | undefined
  >(undefined);

  const [isAddExternalDecisionModalOpen, setIsAddExternalDecisionModalOpen] =
    useState(false);

  const [isEditExternalDecisionModalOpen, setIsEditExternalDecisionModalOpen] =
    useState(false);

  const handleAddExternalDecisionPress = useCallback(() => {
    setIsAddExternalDecisionModalOpen(true);
  }, []);

  const handleExternalDecisionSelect = useCallback(
    (selected: SelectedSchema) => {
      setSelectedSchema(selected);
      setIsAddExternalDecisionModalOpen(false);
      setIsEditExternalDecisionModalOpen(true);
    },
    []
  );

  const handleAddExternalDecisionModalClose = useCallback(() => {
    setIsAddExternalDecisionModalOpen(false);
  }, []);

  const handleEditExternalDecisionModalClose = useCallback(() => {
    setIsEditExternalDecisionModalOpen(false);
  }, []);

  return (
    <FieldSet legend={translate('ExternalDecisions')}>
      <PageSectionContent
        errorMessage={translate('ExternalDecisionsLoadError')}
        error={error}
        isFetching={isFetching}
        isPopulated={isFetched}
      >
        <div className={styles.externalDecisions}>
          {items.map((item) => (
            <ExternalDecision
              key={item.id}
              {...item}
              showPriority={showPriority}
            />
          ))}

          <Card
            className={styles.addExternalDecision}
            onPress={handleAddExternalDecisionPress}
          >
            <div className={styles.center}>
              <Icon name={icons.ADD} size={45} />
            </div>
          </Card>
        </div>

        <AddExternalDecisionModal
          isOpen={isAddExternalDecisionModalOpen}
          onExternalDecisionSelect={handleExternalDecisionSelect}
          onModalClose={handleAddExternalDecisionModalClose}
        />

        <EditExternalDecisionModal
          isOpen={isEditExternalDecisionModalOpen}
          selectedSchema={selectedSchema}
          onModalClose={handleEditExternalDecisionModalClose}
        />
      </PageSectionContent>
    </FieldSet>
  );
}

export default ExternalDecisions;
