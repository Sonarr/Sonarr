import { useCallback, useEffect, useMemo } from 'react';
import { useLocation, useNavigationType } from 'react-router';
import { OnScroll } from 'Components/Scroller/Scroller';
import scrollPositions from 'Helpers/scrollPositions';

function useScrollPosition(key?: string) {
  const { pathname } = useLocation();
  const navigationType = useNavigationType();

  // Reset window scroll on PUSH/REPLACE (mobile's scroll container).
  // Reset the scroll position unless we're going back, this will allow the scroll
  // position to reset when moving forward (PUSH/REPLACE) and restore when
  // moving backwards (POP).
  useEffect(() => {
    if (navigationType !== 'POP') {
      window.scrollTo(0, 0);
    }
  }, [pathname, navigationType]);

  const initialScrollTop = useMemo(
    () => (key && navigationType === 'POP' ? scrollPositions[key] ?? 0 : 0),
    [key, navigationType]
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
