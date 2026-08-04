import {
  autoUpdate,
  flip,
  FloatingPortal,
  shift,
  useDismiss,
  useFloating,
  useInteractions,
} from '@floating-ui/react';
import React, {
  ReactElement,
  useCallback,
  useEffect,
  useId,
  useState,
} from 'react';
import styles from './Menu.css';

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

  const getMenuItems = useCallback(() => {
    const menuContent = document.getElementById(menuContentId);

    if (!menuContent) {
      return [];
    }

    return Array.from(
      menuContent.querySelectorAll<HTMLElement>(
        '[role="menuitem"]:not([disabled]):not([aria-disabled="true"])'
      )
    );
  }, [menuContentId]);

  const closeMenu = useCallback(
    (restoreFocus: boolean) => {
      setIsMenuOpen(false);

      if (restoreFocus) {
        document.getElementById(menuButtonId)?.focus();
      }
    },
    [menuButtonId]
  );

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

  const handleMenuButtonPress = useCallback(() => {
    setIsMenuOpen((isOpen) => !isOpen);
  }, []);

  const handleMenuContentClick = useCallback(
    (event: React.MouseEvent<HTMLElement>) => {
      const target = event.target as Element;

      if (target.closest?.('[role="menuitem"]')) {
        closeMenu(false);
      }
    },
    [closeMenu]
  );

  const handleWindowKeyDown = useCallback(
    (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        event.preventDefault();
        event.stopPropagation();
        closeMenu(true);
        return;
      }

      if (event.key === 'Tab') {
        closeMenu(true);
        return;
      }

      if (
        event.key !== 'ArrowUp' &&
        event.key !== 'ArrowDown' &&
        event.key !== 'Home' &&
        event.key !== 'End'
      ) {
        return;
      }

      const menuItems = getMenuItems();

      if (!menuItems.length) {
        return;
      }

      event.preventDefault();

      const focusedIndex = menuItems.indexOf(
        document.activeElement as HTMLElement
      );
      let nextIndex: number;

      if (event.key === 'Home') {
        nextIndex = 0;
      } else if (event.key === 'End') {
        nextIndex = menuItems.length - 1;
      } else if (event.key === 'ArrowUp') {
        nextIndex =
          focusedIndex <= 0 ? menuItems.length - 1 : focusedIndex - 1;
      } else {
        nextIndex =
          focusedIndex >= menuItems.length - 1 ? 0 : focusedIndex + 1;
      }

      menuItems[nextIndex].focus();
    },
    [closeMenu, getMenuItems]
  );

  const childrenArray = React.Children.toArray(children);
  const button = React.cloneElement(childrenArray[0] as ReactElement, {
    onPress: handleMenuButtonPress,
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
    if (!isMenuOpen) {
      return;
    }

    const frameId = window.requestAnimationFrame(() => {
      getMenuItems()[0]?.focus();
    });

    return () => {
      window.cancelAnimationFrame(frameId);
    };
  }, [getMenuItems, isMenuOpen]);

  useEffect(() => {
    if (!isMenuOpen) {
      return;
    }

    window.addEventListener('keydown', handleWindowKeyDown);

    return () => {
      window.removeEventListener('keydown', handleWindowKeyDown);
    };
  }, [handleWindowKeyDown, isMenuOpen]);

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
    onOpenChange: setIsMenuOpen,
  });

  const dismiss = useDismiss(context, {
    escapeKey: false,
    outsidePressEvent: 'click',
  });

  const { getReferenceProps, getFloatingProps } = useInteractions([dismiss]);

  return (
    <>
      <div
        ref={refs.setReference}
        {...getReferenceProps()}
        className={className}
      >
        {React.cloneElement(button, {
          id: menuButtonId,
          'aria-controls': menuContentId,
          'aria-expanded': isMenuOpen,
          'aria-haspopup': 'menu',
        })}
      </div>

      {isMenuOpen ? (
        <FloatingPortal id="portal-root">
          {React.cloneElement(childrenArray[1] as ReactElement, {
            forwardedRef: refs.setFloating,
            id: menuContentId,
            'aria-labelledby': menuButtonId,
            style: {
              maxHeight: enforceMaxHeight ? maxHeight : undefined,
              ...floatingStyles,
            },
            isOpen: isMenuOpen,
            ...getFloatingProps({ onClick: handleMenuContentClick }),
          })}
        </FloatingPortal>
      ) : null}
    </>
  );
}

export default Menu;
