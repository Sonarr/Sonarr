import { move } from '@dnd-kit/helpers';
import { DragDropProvider, DragEndEvent, DragOverEvent } from '@dnd-kit/react';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes } from 'Helpers/Props';
import { CheckInputChanged, InputChanged } from 'typings/inputs';
import { TableOptionsChangePayload } from 'typings/Table';
import translate from 'Utilities/String/translate';
import Column from '../Column';
import TableOptionsColumn from './TableOptionsColumn';
import styles from './TableOptionsModal.css';

export interface TableOptionsModalProps {
  isOpen: boolean;
  columns: Column[];
  pageSize?: number;
  maxPageSize?: number;
  canModifyColumns?: boolean;
  optionsComponent?: React.ElementType;
  onTableOptionChange: (payload: TableOptionsChangePayload) => void;
  onModalClose: () => void;
}

function TableOptionsModal({
  isOpen,
  columns,
  canModifyColumns = true,
  optionsComponent: OptionsComponent,
  pageSize: propsPageSize,
  maxPageSize = 250,
  onTableOptionChange,
  onModalClose,
}: TableOptionsModalProps) {
  const [pageSize, setPageSize] = useState(propsPageSize);
  const [pageSizeError, setPageSizeError] = useState<string | null>(null);
  const [localColumnNames, setLocalColumnNames] = useState<string[] | null>(
    null
  );

  const hasPageSize = !!propsPageSize;
  const columnsByName = useMemo(
    () => new Map(columns.map((column) => [column.name, column])),
    [columns]
  );
  const displayedColumns = localColumnNames
    ? localColumnNames.map((name) => columnsByName.get(name)!)
    : columns;

  const handlePageSizeChange = useCallback(
    ({ value }: InputChanged<number | null>) => {
      let error: string | null = null;

      if (value === null || value < 5) {
        error = translate('TablePageSizeMinimum', {
          minimumValue: '5',
        });
      } else if (value > maxPageSize) {
        error = translate('TablePageSizeMaximum', {
          maximumValue: `${maxPageSize}`,
        });
      } else {
        onTableOptionChange({ pageSize: value });
      }

      setPageSize(value ?? 0);
      setPageSizeError(error);
    },
    [maxPageSize, onTableOptionChange]
  );

  const handleVisibleChange = useCallback(
    ({ name, value }: CheckInputChanged) => {
      const newColumns = columns.map((column) => {
        if (column.name === name) {
          return {
            ...column,
            isVisible: value,
          };
        }

        return column;
      });

      onTableOptionChange({ columns: newColumns });
    },
    [columns, onTableOptionChange]
  );

  const handleDragStart = useCallback(() => {
    setLocalColumnNames(columns.map((column) => column.name));
  }, [columns]);

  const handleDragOver = useCallback((event: DragOverEvent) => {
    setLocalColumnNames((current) =>
      current ? move(current, event) : current
    );
  }, []);

  const handleDragEnd = useCallback(
    (event: DragEndEvent) => {
      setLocalColumnNames((current) => {
        if (current && !event.canceled) {
          const newColumnNames = move(current, event);

          onTableOptionChange({
            columns: newColumnNames.map((name) => columnsByName.get(name)!),
          });
        }

        return null;
      });
    },
    [columnsByName, onTableOptionChange]
  );

  useEffect(() => {
    setPageSize(propsPageSize);
  }, [propsPageSize]);

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      {isOpen ? (
        <ModalContent onModalClose={onModalClose}>
          <ModalHeader>{translate('TableOptions')}</ModalHeader>

          <ModalBody>
            <Form>
              {hasPageSize ? (
                <FormRow>
                  <FormLabel>{translate('TablePageSize')}</FormLabel>

                  <FormInputHelpText
                    text={translate('TablePageSizeHelpText')}
                  />
                  {pageSizeError ? (
                    <FormInputHelpText text={pageSizeError} isError={true} />
                  ) : null}
                  <FormInput
                    type={inputTypes.NUMBER}
                    name="pageSize"
                    value={pageSize || 0}
                    onChange={handlePageSizeChange}
                  />
                </FormRow>
              ) : null}

              {OptionsComponent ? (
                <OptionsComponent onTableOptionChange={onTableOptionChange} />
              ) : null}

              {canModifyColumns ? (
                <div className={styles.columnsSection}>
                  <FormLabel>{translate('TableColumns')}</FormLabel>
                  <FormInputHelpText text={translate('TableColumnsHelpText')} />

                  <DragDropProvider
                    onDragStart={handleDragStart}
                    onDragOver={handleDragOver}
                    onDragEnd={handleDragEnd}
                  >
                    <div className={styles.columns}>
                      {displayedColumns.map((column, index) => {
                        const {
                          name,
                          label,
                          columnLabel,
                          isVisible,
                          isModifiable = 'enabled',
                        } = column;

                        return (
                          <TableOptionsColumn
                            key={name}
                            name={name}
                            label={columnLabel ?? label}
                            isVisible={isVisible}
                            isModifiable={isModifiable}
                            index={index}
                            onVisibleChange={handleVisibleChange}
                          />
                        );
                      })}
                    </div>
                  </DragDropProvider>
                </div>
              ) : null}
            </Form>
          </ModalBody>
          <ModalFooter>
            <Button onPress={onModalClose}>{translate('Close')}</Button>
          </ModalFooter>
        </ModalContent>
      ) : null}
    </Modal>
  );
}

export default TableOptionsModal;
