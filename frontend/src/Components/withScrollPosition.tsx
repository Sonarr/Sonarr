import PropTypes from 'prop-types';
import React from 'react';
import scrollPositions from 'Store/scrollPositions';

function withScrollPosition(WrappedComponent, scrollPositionKey) {
  function ScrollPosition(props) {
    const { history } = props;

    const initialScrollTop =
      history.action === 'POP' ||
      (history.location.state && history.location.state.restoreScrollPosition)
        ? scrollPositions[scrollPositionKey]
        : 0;

    return <WrappedComponent {...props} initialScrollTop={initialScrollTop} />;
  }

  ScrollPosition.propTypes = {
    history: PropTypes.object.isRequired,
  };

  return ScrollPosition;
}

export default withScrollPosition;
