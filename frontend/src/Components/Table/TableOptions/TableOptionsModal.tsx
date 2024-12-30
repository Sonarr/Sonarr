import _ from 'lodash';
import { HTML5toTouch } from 'rdndmb-html5-to-touch';
import React, { useCallback, useEffect, useState } from 'react';
import { DndProvider } from 'react-dnd-multi-backend';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
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

interface TableOptionsModalProps {
  isOpen: boolean;
  columns: Column[];
  pageSize?: number;
  maxPageSize?: number;
  canModifyColumns: boolean;
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
  const [dragIndex, setDragIndex] = useState<number | null>(null);
  const [dropIndex, setDropIndex] = useState<number | null>(null);

  const hasPageSize = !!propsPageSize;
  const isDragging = dropIndex !== null;
  const isDraggingUp =
    isDragging &&
    dropIndex != null &&
    dragIndex != null &&
    dropIndex < dragIndex;
  const isDraggingDown =
    isDragging &&
    dropIndex != null &&
    dragIndex != null &&
    dropIndex > dragIndex;

  const handlePageSizeChange = useCallback(
    ({ value }: InputChanged<number>) => {
      let error: string | null = null;

      if (value < 5) {
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

      setPageSize(value);
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

  const handleColumnDragMove = useCallback(
    (newDragIndex: number, newDropIndex: number) => {
      setDropIndex(newDropIndex);
      setDragIndex(newDragIndex);
    },
    []
  );

  const handleColumnDragEnd = useCallback(
    (didDrop: boolean) => {
      if (didDrop && dragIndex && dropIndex !== null) {
        const newColumns = [...columns];
        const items = newColumns.splice(dragIndex, 1);
        newColumns.splice(dropIndex, 0, items[0]);

        onTableOptionChange({ columns: newColumns });
      }

      setDragIndex(null);
      setDropIndex(null);
    },
    [dragIndex, dropIndex, columns, onTableOptionChange]
  );

  useEffect(() => {
    setPageSize(propsPageSize);
  }, [propsPageSize]);

  return (
    <DndProvider options={HTML5toTouch}>
      <Modal isOpen={isOpen} onModalClose={onModalClose}>
        {isOpen ? (
          <ModalContent onModalClose={onModalClose}>
            <ModalHeader>{translate('TableOptions')}</ModalHeader>

            <ModalBody>
              <Form>
                {hasPageSize ? (
                  <FormGroup>
                    <FormLabel>{translate('TablePageSize')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.NUMBER}
                      name="pageSize"
                      value={pageSize || 0}
                      helpText={translate('TablePageSizeHelpText')}
                      errors={
                        pageSizeError ? [{ message: pageSizeError }] : undefined
                      }
                      onChange={handlePageSizeChange}
                    />
                  </FormGroup>
                ) : null}

                {OptionsComponent ? (
                  <OptionsComponent onTableOptionChange={onTableOptionChange} />
                ) : null}

                {canModifyColumns ? (
                  <FormGroup>
                    <FormLabel>{translate('TableColumns')}</FormLabel>

                    <div>
                      <FormInputHelpText
                        text={translate('TableColumnsHelpText')}
                      />

                      <div className={styles.columns}>
                        {columns.map((column, index) => {
                          const {
                            name,
                            label,
                            columnLabel,
                            isVisible,
                            isModifiable = true,
                          } = column;

                          return (
                            <TableOptionsColumn
                              key={name}
                              name={name}
                              label={columnLabel ?? label}
                              isVisible={isVisible}
                              isModifiable={isModifiable}
                              index={index}
                              isDraggingUp={isDraggingUp}
                              isDraggingDown={isDraggingDown}
                              onVisibleChange={handleVisibleChange}
                              onColumnDragMove={handleColumnDragMove}
                              onColumnDragEnd={handleColumnDragEnd}
                            />
                          );
                        })}
                      </div>
                    </div>
                  </FormGroup>
                ) : null}
              </Form>
            </ModalBody>
            <ModalFooter>
              <Button onPress={onModalClose}>{translate('Close')}</Button>
            </ModalFooter>
          </ModalContent>
        ) : null}
      </Modal>
    </DndProvider>
  );
}

TableOptionsModal.defaultProps = {
  canModifyColumns: true,
};

export default TableOptionsModal;
