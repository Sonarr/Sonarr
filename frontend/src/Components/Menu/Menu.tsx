import {
  autoUpdate,
  flip,
  FloatingPortal,
  shift,
  useClick,
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
  const [maxHeight, setMaxHeight] = useState(0);
  const [isMenuOpen, setIsMenuOpen] = useState(false);

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

  const childrenArray = React.Children.toArray(children);
  const button = React.cloneElement(childrenArray[0] as ReactElement, {
    onPress: handleMenuButtonPress,
  });

  const handleFloaterPress = useCallback((_event: MouseEvent) => {
    // TODO: Menu items should handle closing when they are clicked.
    // This is handled before the menu item click event is handled, so wait 100ms before closing.
    setTimeout(() => {
      setIsMenuOpen(false);
    }, 100);

    return true;
  }, []);

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

  const click = useClick(context);
  const dismiss = useDismiss(context, {
    outsidePress: handleFloaterPress,
  });

  const { getReferenceProps, getFloatingProps } = useInteractions([
    click,
    dismiss,
  ]);

  return (
    <>
      <div
        ref={refs.setReference}
        {...getReferenceProps()}
        id={menuButtonId}
        className={className}
      >
        {button}
      </div>

      {isMenuOpen ? (
        <FloatingPortal id="portal-root">
          {React.cloneElement(childrenArray[1] as ReactElement, {
            forwardedRef: refs.setFloating,
            style: {
              maxHeight,
              ...floatingStyles,
            },
            isOpen: isMenuOpen,
            ...getFloatingProps(),
          })}
        </FloatingPortal>
      ) : null}
    </>
  );
}

export default Menu;
