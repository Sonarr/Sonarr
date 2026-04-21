import { useCallback, useEffect, useMemo } from 'react';
import { useHistory, useLocation } from 'react-router';
import { OnScroll } from 'Components/Scroller/Scroller';
import scrollPositions from 'Store/scrollPositions';

function useScrollPosition(key?: string) {
  const { pathname } = useLocation();
  const { action } = useHistory();

  // Reset window scroll on PUSH/REPLACE (mobile's scroll container).
  // Reset the scroll position unless we're going back, this will allow the scroll
  // position to reset when moving forward (PUSH/REPLACE) and restore when
  // moving backwards (POP).
  useEffect(() => {
    if (action !== 'POP') {
      window.scrollTo(0, 0);
    }
  }, [pathname, action]);

  const initialScrollTop = useMemo(
    () => (key && action === 'POP' ? scrollPositions[key] ?? 0 : 0),
    [key, action]
  );

  const onScroll = useCallback(
    ({ scrollTop }: OnScroll) => {
      if (key) {
        scrollPositions[key] = scrollTop;
      }
    },
    [key]
  );

  return { initialScrollTop, onScroll };
}

export default useScrollPosition;
