import PropTypes from 'prop-types';
import React from 'react';

function withCurrentPage(WrappedComponent) {
  function CurrentPage(props) {
    const {
      history
    } = props;

    return (
      <WrappedComponent
        {...props}
        useCurrentPage={history.action === 'POP'}
      />
    );
  }

  CurrentPage.propTypes = {
    history: PropTypes.object.isRequired
  };

  return CurrentPage;
}

export default withCurrentPage;
