import React, { ReactElement, useCallback, useState } from 'react';
import { LinkProps } from 'Components/Link/Link';
import Column from '../Column';
import TableOptionsModal, { TableOptionsModalProps } from './TableOptionsModal';

interface TableOptionsModalWrapperProps
  extends Omit<TableOptionsModalProps, 'isOpen' | 'onModalClose'> {
  columns: Column[];
  children: ReactElement<LinkProps>;
}

function TableOptionsModalWrapper({
  columns,
  children,
  ...otherProps
}: TableOptionsModalWrapperProps) {
  const [isTableOptionsModalOpen, setIsTableOptionsModalOpen] = useState(false);

  const handleTableOptionsPress = useCallback(() => {
    setIsTableOptionsModalOpen(true);
  }, []);

  const handleTableOptionsModalClose = useCallback(() => {
    setIsTableOptionsModalOpen(false);
  }, []);

  return (
    <>
      {React.isValidElement(children)
        ? React.cloneElement(children, { onPress: handleTableOptionsPress })
        : null}

      <TableOptionsModal
        {...otherProps}
        isOpen={isTableOptionsModalOpen}
        columns={columns}
        onModalClose={handleTableOptionsModalClose}
      />
    </>
  );
}

export default TableOptionsModalWrapper;
