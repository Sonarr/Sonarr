import React, {
  ReactElement,
  useCallback,
  useEffect,
  useId,
  useRef,
  useState,
} from 'react';
import { Manager, Popper, PopperProps, Reference } from 'react-popper';
import Portal from 'Components/Portal';
import styles from './Menu.css';

const sharedPopperOptions = {
  modifiers: {
    preventOverflow: {
      padding: 0,
    },
    flip: {
      padding: 0,
    },
  },
};

const popperOptions: {
  right: Partial<PopperProps>;
  left: Partial<PopperProps>;
} = {
  right: {
    ...sharedPopperOptions,
    placement: 'bottom-end',
  },

  left: {
    ...sharedPopperOptions,
    placement: 'bottom-start',
  },
};

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
  const updater = useRef<(() => void) | null>(null);
  const menuButtonId = useId();
  const menuContentId = useId();
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

  const handleWindowClick = useCallback(
    (event: MouseEvent) => {
      const menuButton = document.getElementById(menuButtonId);

      if (!menuButton) {
        return;
      }

      if (!menuButton.contains(event.target as Node)) {
        setIsMenuOpen(false);
      }
    },
    [menuButtonId]
  );

  const handleTouchStart = useCallback(
    (event: TouchEvent) => {
      const menuButton = document.getElementById(menuButtonId);
      const menuContent = document.getElementById(menuContentId);

      if (!menuButton || !menuContent) {
        return;
      }

      if (event.targetTouches.length !== 1) {
        return;
      }

      const target = event.targetTouches[0].target;

      if (
        !menuButton.contains(target as Node) &&
        !menuContent.contains(target as Node)
      ) {
        setIsMenuOpen(false);
      }
    },
    [menuButtonId, menuContentId]
  );

  const handleWindowResize = useCallback(() => {
    updateMaxHeight();
  }, [updateMaxHeight]);

  const handleWindowScroll = useCallback(() => {
    if (isMenuOpen) {
      updateMaxHeight();
    }
  }, [isMenuOpen, updateMaxHeight]);

  const handleMenuButtonPress = useCallback(() => {
    setIsMenuOpen((isOpen) => !isOpen);
  }, []);

  const childrenArray = React.Children.toArray(children);
  const button = React.cloneElement(childrenArray[0] as ReactElement, {
    onPress: handleMenuButtonPress,
  });

  useEffect(() => {
    if (enforceMaxHeight) {
      updateMaxHeight();
    }
  }, [enforceMaxHeight, updateMaxHeight]);

  useEffect(() => {
    if (updater.current && isMenuOpen) {
      updater.current();
    }
  }, [isMenuOpen]);

  useEffect(() => {
    // Listen to resize events on the window and scroll events
    // on all elements to ensure the menu is the best size possible.
    // Listen for click events on the window to support closing the
    // menu on clicks outside.

    if (!isMenuOpen) {
      return;
    }

    window.addEventListener('resize', handleWindowResize);
    window.addEventListener('scroll', handleWindowScroll, { capture: true });
    window.addEventListener('click', handleWindowClick);
    window.addEventListener('touchstart', handleTouchStart);

    return () => {
      window.removeEventListener('resize', handleWindowResize);
      window.removeEventListener('scroll', handleWindowScroll, {
        capture: true,
      });
      window.removeEventListener('click', handleWindowClick);
      window.removeEventListener('touchstart', handleTouchStart);
    };
  }, [
    isMenuOpen,
    handleWindowResize,
    handleWindowScroll,
    handleWindowClick,
    handleTouchStart,
  ]);

  return (
    <Manager>
      <Reference>
        {({ ref }) => (
          <div ref={ref} id={menuButtonId} className={className}>
            {button}
          </div>
        )}
      </Reference>

      <Portal>
        <Popper {...popperOptions[alignMenu]}>
          {({ ref, style, scheduleUpdate }) => {
            updater.current = scheduleUpdate;

            return React.cloneElement(childrenArray[1] as ReactElement, {
              forwardedRef: ref,
              style: {
                ...style,
                maxHeight,
              },
              isOpen: isMenuOpen,
            });
          }}
        </Popper>
      </Portal>
    </Manager>
  );
}

export default Menu;
