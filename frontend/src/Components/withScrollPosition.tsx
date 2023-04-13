import React from 'react';
import { RouteComponentProps } from 'react-router-dom';
import scrollPositions from 'Store/scrollPositions';

interface WrappedComponentProps {
  initialScrollTop: number;
}

interface ScrollPositionProps {
  history: RouteComponentProps['history'];
  location: RouteComponentProps['location'];
  match: RouteComponentProps['match'];
}

function withScrollPosition(
  WrappedComponent: React.FC<WrappedComponentProps>,
  scrollPositionKey: string
) {
  function ScrollPosition(props: ScrollPositionProps) {
    const { history } = props;

    const initialScrollTop =
      history.action === 'POP' ? scrollPositions[scrollPositionKey] : 0;

    return <WrappedComponent {...props} initialScrollTop={initialScrollTop} />;
  }

  return ScrollPosition;
}

export default withScrollPosition;
