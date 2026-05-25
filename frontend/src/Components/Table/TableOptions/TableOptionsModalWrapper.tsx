import React, { ReactElement, useCallback, useState } from 'react';
import { LinkProps } from 'Components/Link/Link';
import Column from '../Column';
import TableOptionsModal, { TableOptionsModalProps } from './TableOptionsModal';

interface TableOptionsModalWrapperProps
  extends Omit<TableOptionsModalProps, 'isOpen' | 'onModalClose'> {
  columns: Column[];
  children: ReactElement<LinkProps>;
  // Optional controlled mode — parent owns state so the overflow popup entry can trigger the same modal.
  isOpen?: boolean;
  onPress?: () => void;
  onModalClose?: () => void;
}

function TableOptionsModalWrapper({
  columns,
  children,
  isOpen,
  onPress,
  onModalClose,
  ...otherProps
}: TableOptionsModalWrapperProps) {
  const [internalOpen, setInternalOpen] = useState(false);

  const isControlled = isOpen !== undefined;
  const effectiveOpen = isControlled ? isOpen : internalOpen;

  const handleTableOptionsPress = useCallback(() => {
    if (isControlled) {
      onPress?.();
    } else {
      setInternalOpen(true);
    }
  }, [isControlled, onPress]);

  const handleTableOptionsModalClose = useCallback(() => {
    if (isControlled) {
      onModalClose?.();
    } else {
      setInternalOpen(false);
    }
  }, [isControlled, onModalClose]);

  return (
    <>
      {React.isValidElement(children)
        ? React.cloneElement(children, { onPress: handleTableOptionsPress })
        : null}

      <TableOptionsModal
        {...otherProps}
        isOpen={effectiveOpen}
        columns={columns}
        onModalClose={handleTableOptionsModalClose}
      />
    </>
  );
}

export default TableOptionsModalWrapper;
