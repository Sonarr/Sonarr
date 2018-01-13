import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import Alert from 'Components/Alert';

function Form({ children, validationErrors, validationWarnings, ...otherProps }) {
  return (
    <div>
      <div>
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
      </div>

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
