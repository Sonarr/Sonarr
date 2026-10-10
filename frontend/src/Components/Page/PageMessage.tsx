import React, { ReactNode } from 'react';
import Alert from 'Components/Alert';
import { kinds } from 'Helpers/Props';

interface PageMessageProps {
  children: ReactNode;
}

function PageMessage({ children }: PageMessageProps) {
  return <Alert kind={kinds.INFO}>{children}</Alert>;
}

export default PageMessage;
