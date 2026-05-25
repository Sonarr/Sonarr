import React, { useCallback, useState } from 'react';
import ParseModal from './ParseModal';

interface ParseModalControls {
  open: () => void;
  modal: React.ReactElement;
}

export default function useParseModal(): ParseModalControls {
  const [isOpen, setIsOpen] = useState(false);
  const open = useCallback(() => setIsOpen(true), []);
  const close = useCallback(() => setIsOpen(false), []);
  return {
    open,
    modal: <ParseModal isOpen={isOpen} onModalClose={close} />,
  };
}
