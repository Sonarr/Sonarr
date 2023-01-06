import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import { kinds } from 'Helpers/Props';
import styles from './Form.css';

function Form(props) {
  const {
    children,
    validationErrors,
    validationWarnings,
    // eslint-disable-next-line no-unused-vars
    ...otherProps
  } = props;

  return (
    <div>
      {
        validationErrors.length || validationWarnings.length ?
          <div className={styles.validationFailures}>
            {
              validationErrors.map((error, index) => {
                return (
                  <Alert
                    key={index}
                    kind={kinds.DANGER}
                  >
                    {error.errorMessage}
                  </Alert>
                );
              })
            }

            {
              validationWarnings.map((warning, index) => {
                return (
                  <Alert
                    key={index}
                    kind={kinds.WARNING}
                  >
                    {warning.errorMessage}
                  </Alert>
                );
              })
            }
          </div> :
          null
      }

      {children}
    </div>
  );
}

Form.propTypes = {
  children: PropTypes.node.isRequired,
  validationErrors: PropTypes.arrayOf(PropTypes.object).isRequired,
  validationWarnings: PropTypes.arrayOf(PropTypes.object).isRequired
};

Form.defaultProps = {
  validationErrors: [],
  validationWarnings: []
};

export default Form;
