import PropTypes from 'prop-types';
import React from 'react';
import scrollPositions from 'Store/scrollPositions';

function withScrollPosition(WrappedComponent, scrollPositionKey) {
  function ScrollPosition(props) {
    const {
      history
    } = props;

    const scrollTop = history.action === 'POP' ?
      scrollPositions[scrollPositionKey] :
      0;

    return (
      <WrappedComponent
        {...props}
        scrollTop={scrollTop}
      />
    );
  }

  ScrollPosition.propTypes = {
    history: PropTypes.object.isRequired
  };

  return ScrollPosition;
}

export default withScrollPosition;
