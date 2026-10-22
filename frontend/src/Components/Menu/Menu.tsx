import {
  autoUpdate,
  flip,
  FloatingFocusManager,
  FloatingPortal,
  shift,
  useDismiss,
  useFloating,
  useInteractions,
  useRole,
} from '@floating-ui/react';
import React, {
  ReactElement,
  useCallback,
  useEffect,
  useId,
  useMemo,
  useRef,
  useState,
} from 'react';
import MenuContext from './MenuContext';
import styles from './Menu.module.css';

interface MenuProps {
  className?: string;
  children: React.ReactNode;
  alignMenu?: 'left' | 'right';
  enforceMaxHeight?: boolean;
}

function Menu({
  className = styles.menu,
  children,
  alignMenu = 'left',
  enforceMaxHeight = true,
}: MenuProps) {
  const menuButtonId = useId();
  const menuContentId = useId();
  const [maxHeight, setMaxHeight] = useState(0);
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const shouldRestoreFocus = useRef(false);

  const updateMaxHeight = useCallback(() => {
    const menuButton = document.getElementById(menuButtonId);

    if (!menuButton) {
      setMaxHeight(0);

      return;
    }

    const { bottom } = menuButton.getBoundingClientRect();
    const height = window.innerHeight - bottom;

    setMaxHeight(height);
  }, [menuButtonId]);

  const focusMenuButton = useCallback(() => {
    if (document.activeElement !== document.body) {
      return;
    }

    document
      .getElementById(menuButtonId)
      ?.querySelector<HTMLElement>('button, a[href]')
      ?.focus();
  }, [menuButtonId]);

  useEffect(() => {
    if (!isMenuOpen && shouldRestoreFocus.current) {
      shouldRestoreFocus.current = false;
      const focusTimeout = window.setTimeout(focusMenuButton, 100);

      return () => window.clearTimeout(focusTimeout);
    }

    return undefined;
  }, [focusMenuButton, isMenuOpen]);

  const handleMenuButtonPress = useCallback(() => {
    setIsMenuOpen((isOpen) => !isOpen);
  }, []);

  const childrenArray = React.Children.toArray(children);
  const button = React.cloneElement(childrenArray[0] as ReactElement, {
    onPress: handleMenuButtonPress,
    'aria-haspopup': 'menu',
    'aria-expanded': isMenuOpen,
    'aria-controls': isMenuOpen ? menuContentId : undefined,
  });

  const handleWindowResize = useCallback(() => {
    updateMaxHeight();
  }, [updateMaxHeight]);

  const handleWindowScroll = useCallback(() => {
    if (isMenuOpen) {
      updateMaxHeight();
    }
  }, [isMenuOpen, updateMaxHeight]);

  useEffect(() => {
    if (enforceMaxHeight) {
      updateMaxHeight();
    }
  }, [enforceMaxHeight, updateMaxHeight]);

  useEffect(() => {
    // Listen to resize events on the window and scroll events
    // on all elements to ensure the menu is the best size possible.

    if (!isMenuOpen) {
      return;
    }

    window.addEventListener('resize', handleWindowResize);
    window.addEventListener('scroll', handleWindowScroll, { capture: true });

    return () => {
      window.removeEventListener('resize', handleWindowResize);
      window.removeEventListener('scroll', handleWindowScroll, {
        capture: true,
      });
    };
  }, [isMenuOpen, handleWindowResize, handleWindowScroll]);

  const handleOpenChange = useCallback(
    (isOpen: boolean, _event?: Event, reason?: string) => {
      setIsMenuOpen(isOpen);

      if (!isOpen && reason === 'escape-key') {
        shouldRestoreFocus.current = true;
      }
    },
    []
  );

  const { refs, context, floatingStyles } = useFloating({
    middleware: [
      flip({
        crossAxis: false,
        mainAxis: true,
      }),
      // offset({ mainAxis: 10 }),
      shift(),
    ],
    open: isMenuOpen,
    placement: alignMenu === 'left' ? 'bottom-start' : 'bottom-end',
    whileElementsMounted: autoUpdate,
    onOpenChange: handleOpenChange,
  });

  const dismiss = useDismiss(context, {
    outsidePressEvent: 'click',
  });
  const role = useRole(context, { role: 'menu' });

  const { getFloatingProps } = useInteractions([dismiss, role]);

  const handleMenuKeyDown = useCallback(
    (event: React.KeyboardEvent<HTMLDivElement>) => {
      if (!['ArrowDown', 'ArrowUp', 'Home', 'End'].includes(event.key)) {
        return;
      }

      const items = Array.from(
        event.currentTarget.querySelectorAll<HTMLElement>(
          '[role^="menuitem"]:not(:disabled):not([aria-disabled="true"])'
        )
      );

      if (!items.length) {
        return;
      }

      event.preventDefault();

      const currentIndex = items.indexOf(document.activeElement as HTMLElement);
      let nextIndex = currentIndex;

      if (event.key === 'Home') {
        nextIndex = 0;
      } else if (event.key === 'End') {
        nextIndex = items.length - 1;
      } else if (event.key === 'ArrowDown') {
        nextIndex = currentIndex < items.length - 1 ? currentIndex + 1 : 0;
      } else {
        nextIndex = currentIndex > 0 ? currentIndex - 1 : items.length - 1;
      }

      items[nextIndex].focus();
    },
    []
  );

  const closeMenu = useCallback(() => {
    shouldRestoreFocus.current = true;
    setIsMenuOpen(false);
  }, []);

  const menuContext = useMemo(() => ({ closeMenu }), [closeMenu]);

  return (
    <>
      <div ref={refs.setReference} id={menuButtonId} className={className}>
        {button}
      </div>

      {isMenuOpen ? (
        <FloatingPortal id="portal-root">
          <FloatingFocusManager
            context={context}
            modal={false}
            returnFocus={false}
          >
            <MenuContext.Provider value={menuContext}>
              {React.cloneElement(childrenArray[1] as ReactElement, {
                forwardedRef: refs.setFloating,
                style: {
                  maxHeight: enforceMaxHeight ? maxHeight : undefined,
                  ...floatingStyles,
                },
                isOpen: isMenuOpen,
                ...getFloatingProps({ onKeyDown: handleMenuKeyDown }),
                id: menuContentId,
              })}
            </MenuContext.Provider>
          </FloatingFocusManager>
        </FloatingPortal>
      ) : null}
    </>
  );
}

export default Menu;
