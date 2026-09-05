import React, { useMemo } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import { SelectedSchema } from 'Settings/useProviderSchema';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';
import AddImportListItem from './AddImportListItem';
import { ImportListModel, useImportListSchema } from './useImportLists';
import styles from './AddImportListModalContent.css';

export interface AddImportListModalContentProps {
  onImportListSelect: (selectedSchema: SelectedSchema) => void;
  onModalClose: () => void;
}

function getListGroupTitle(typeOfList: string) {
  // Does not need to be translated as it is a proper name.
  if (typeOfList === 'tmdb') {
    return 'TMDb';
  }

  return translate('TypeOfList', {
    typeOfList: titleCase(typeOfList),
  });
}

function AddImportListModalContent({
  onImportListSelect,
  onModalClose,
}: AddImportListModalContentProps) {
  const { isSchemaLoading, isSchemaFetched, schemaError, schema } =
    useImportListSchema();

  const listGroups = useMemo(() => {
    const result = schema.reduce<Record<string, ImportListModel[]>>(
      (acc, item) => {
        if (!acc[item.listType]) {
          acc[item.listType] = [];
        }

        acc[item.listType].push(item);

        return acc;
      },
      {}
    );

    Object.keys(result).forEach((key) => {
      result[key].sort((a, b) => {
        return a.listOrder - b.listOrder;
      });
    });

    return result;
  }, [schema]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('AddImportList')}</ModalHeader>

      <ModalBody>
        {isSchemaLoading ? <LoadingIndicator /> : null}

        {!isSchemaLoading && !!schemaError ? (
          <Alert kind={kinds.DANGER}>{translate('AddListError')}</Alert>
        ) : null}

        {isSchemaFetched && !schemaError ? (
          <div>
            <Alert kind={kinds.INFO}>
              <div>{translate('SupportedListsSeries')}</div>
              <div>{translate('SupportedListsMoreInfo')}</div>
            </Alert>
            {Object.keys(listGroups).map((key) => {
              return (
                <FieldSet key={key} legend={getListGroupTitle(key)}>
                  <div className={styles.lists}>
                    {listGroups[key].map((list) => {
                      return (
                        <AddImportListItem
                          key={list.implementation}
                          {...list}
                          implementation={list.implementation}
                          onImportListSelect={onImportListSelect}
                        />
                      );
                    })}
                  </div>
                </FieldSet>
              );
            })}
          </div>
        ) : null}
      </ModalBody>
      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default AddImportListModalContent;
