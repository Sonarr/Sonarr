import React, { ReactNode } from 'react';
import Alert from 'Components/Alert';
import { kinds } from 'Helpers/Props';
import { ValidationError, ValidationWarning } from 'typings/pending';
import styles from './Form.css';

export interface FormProps {
  id?: string;
  children: ReactNode;
  validationErrors?: ValidationError[];
  validationWarnings?: ValidationWarning[];
}

function Form({
  id,
  children,
  validationErrors = [],
  validationWarnings = [],
}: FormProps) {
  return (
    <div id={id}>
      {validationErrors.length || validationWarnings.length ? (
        <div className={styles.validationFailures}>
          {validationErrors.map((error, index) => {
            return (
              <Alert key={index} kind={kinds.DANGER}>
                {error.errorMessage}
              </Alert>
            );
          })}

          {validationWarnings.map((warning, index) => {
            return (
              <Alert key={index} kind={kinds.WARNING}>
                {warning.errorMessage}
              </Alert>
            );
          })}
        </div>
      ) : null}

      {children}
    </div>
  );
}

export default Form;
