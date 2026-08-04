import classNames from 'classnames';
import elementClass from 'element-class';
import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import FocusLock from 'react-focus-lock';
import { useLocation } from 'react-router';
import QueueStatus from 'Activity/Queue/Status/QueueStatus';
import {
  setIsSidebarVisible,
  useAppDimension,
  useAppValue,
} from 'App/appStore';
import { IconName } from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import OverlayScroller from 'Components/Scroller/OverlayScroller';
import Scroller from 'Components/Scroller/Scroller';
import { icons } from 'Helpers/Props';
import dimensions from 'Styles/Variables/dimensions';
import HealthStatus from 'System/Status/Health/HealthStatus';
import * as keyCodes from 'Utilities/Constants/keyCodes';
import { setScrollLock } from 'Utilities/scrollLock';
import translate from 'Utilities/String/translate';
import Messages from './Messages/Messages';
import PageSidebarItem from './PageSidebarItem';
import styles from './PageSidebar.css';

const SIDEBAR_WIDTH = parseInt(dimensions.sidebarWidth);
const MAX_MOBILE_SIDEBAR_WIDTH = 320;
const MIN_MOBILE_SIDEBAR_WIDTH = 260;
const MOBILE_SIDEBAR_VIEWPORT_RATIO = 0.86;
const MOBILE_EDGE_SWIPE_WIDTH = 32;

interface SidebarTransform {
  transition: string;
  transform: number;
}

type SidebarContainerStyle = React.CSSProperties & {
  '--mobileSidebarWidth': string;
};

function getMobileSidebarWidth() {
  return Math.min(
    MAX_MOBILE_SIDEBAR_WIDTH,
    Math.max(
      MIN_MOBILE_SIDEBAR_WIDTH,
      Math.round(window.innerWidth * MOBILE_SIDEBAR_VIEWPORT_RATIO)
    )
  );
}

interface SidebarItem {
  iconName?: IconName;
  title: string | (() => string);
  to: string;
  alias?: string;
  isActive?: boolean;
  isActiveParent?: boolean;
  isParentItem?: boolean;
  isChildItem?: boolean;
  statusComponent?: React.ElementType;
  children?: {
    title: string | (() => string);
    to: string;
    statusComponent?: React.ElementType;
  }[];
}

const LINKS: SidebarItem[] = [
  {
    iconName: icons.SERIES_CONTINUING,
    title: () => translate('Series'),
    to: '/',
    alias: '/series',
    children: [
      {
        title: () => translate('AddNew'),
        to: '/add/new',
      },
      {
        title: () => translate('LibraryImport'),
        to: '/add/import',
      },
      {
        title: () => translate('Statistics'),
        to: '/statistics',
      },
    ],
  },

  {
    iconName: icons.CALENDAR,
    title: () => translate('Calendar'),
    to: '/calendar',
  },

  {
    iconName: icons.ACTIVITY,
    title: () => translate('Activity'),
    to: '/activity/queue',
    children: [
      {
        title: () => translate('Queue'),
        to: '/activity/queue',
        statusComponent: QueueStatus,
      },
      {
        title: () => translate('History'),
        to: '/activity/history',
      },
      {
        title: () => translate('Blocklist'),
        to: '/activity/blocklist',
      },
    ],
  },

  {
    iconName: icons.WARNING,
    title: () => translate('Wanted'),
    to: '/wanted/missing',
    children: [
      {
        title: () => translate('Missing'),
        to: '/wanted/missing',
      },
      {
        title: () => translate('CutoffUnmet'),
        to: '/wanted/cutoffunmet',
      },
    ],
  },

  {
    iconName: icons.SETTINGS,
    title: () => translate('Settings'),
    to: '/settings',
    children: [
      {
        title: () => translate('MediaManagement'),
        to: '/settings/mediamanagement',
      },
      {
        title: () => translate('Profiles'),
        to: '/settings/profiles',
      },
      {
        title: () => translate('Quality'),
        to: '/settings/quality',
      },
      {
        title: () => translate('CustomFormats'),
        to: '/settings/customformats',
      },
      {
        title: () => translate('Indexers'),
        to: '/settings/indexers',
      },
      {
        title: () => translate('DownloadClients'),
        to: '/settings/downloadclients',
      },
      {
        title: () => translate('ImportLists'),
        to: '/settings/importlists',
      },
      {
        title: () => translate('Connect'),
        to: '/settings/connect',
      },
      {
        title: () => translate('Metadata'),
        to: '/settings/metadata',
      },
      {
        title: () => translate('MetadataSource'),
        to: '/settings/metadatasource',
      },
      {
        title: () => translate('Tags'),
        to: '/settings/tags',
      },
      {
        title: () => translate('General'),
        to: '/settings/general',
      },
      {
        title: () => translate('Ui'),
        to: '/settings/ui',
      },
    ],
  },

  {
    iconName: icons.SYSTEM,
    title: () => translate('System'),
    to: '/system/status',
    children: [
      {
        title: () => translate('Status'),
        to: '/system/status',
        statusComponent: HealthStatus,
      },
      {
        title: () => translate('Tasks'),
        to: '/system/tasks',
      },
      {
        title: () => translate('Backup'),
        to: '/system/backup',
      },
      {
        title: () => translate('Updates'),
        to: '/system/updates',
      },
      {
        title: () => translate('Events'),
        to: '/system/events',
      },
      {
        title: () => translate('LogFiles'),
        to: '/system/logs/files',
      },
    ],
  },
];

function hasActiveChildLink(link: SidebarItem, pathname: string) {
  const children = link.children;

  if (!children || !children.length) {
    return false;
  }

  return children.some((child) => {
    return child.to === pathname;
  });
}

function PageSidebar() {
  const isSidebarVisible = useAppValue('isSidebarVisible');
  const isSmallScreen = useAppDimension('isSmallScreen');
  const location = useLocation();
  const { pathname } = location;
  const sidebarRef = useRef<HTMLElement>(null);
  const touchStartX = useRef<number | null>(null);
  const touchStartY = useRef<number | null>(null);
  const previousPathname = useRef(pathname);
  const previousMobileOpen = useRef(isSmallScreen && isSidebarVisible);
  const focusFrame = useRef<number | null>(null);
  const initialSidebarWidth = isSmallScreen
    ? getMobileSidebarWidth()
    : SIDEBAR_WIDTH;

  const [sidebarTransform, setSidebarTransform] = useState<SidebarTransform>({
    transition: 'none',
    transform:
      !isSmallScreen || isSidebarVisible ? 0 : initialSidebarWidth * -1,
  });

  const sidebarWidth = isSmallScreen
    ? getMobileSidebarWidth()
    : SIDEBAR_WIDTH;

  const activeParent = useMemo(() => {
    return (
      LINKS.find((link) => {
        if (link.to && link.to === pathname) {
          return true;
        }

        const children = link.children;

        if (children) {
          const matchingChild = children.find((childLink) => {
            return pathname.startsWith(childLink.to);
          });

          if (matchingChild) {
            return matchingChild;
          }
        }

        if (
          (link.to !== '/' && pathname.startsWith(link.to)) ||
          (link.alias && pathname.startsWith(link.alias))
        ) {
          return true;
        }

        return false;
      })?.to ?? LINKS[0].to
    );
  }, [pathname]);

  const setSidebarVisible = useCallback((isVisible: boolean) => {
    setIsSidebarVisible({ isSidebarVisible: isVisible });
  }, []);

  const setSidebarPosition = useCallback(
    (isVisible: boolean, transition = 'transform 200ms ease-out') => {
      const width = isSmallScreen
        ? getMobileSidebarWidth()
        : SIDEBAR_WIDTH;

      setSidebarTransform({
        transition,
        transform: !isSmallScreen || isVisible ? 0 : width * -1,
      });
    },
    [isSmallScreen]
  );

  const handleClose = useCallback(() => {
    setSidebarVisible(false);
  }, [setSidebarVisible]);

  const handleItemPress = useCallback(() => {
    if (isSmallScreen) {
      setSidebarVisible(false);
    }
  }, [isSmallScreen, setSidebarVisible]);

  const handleKeyDown = useCallback(
    (event: KeyboardEvent) => {
      if (
        event.keyCode === keyCodes.ESCAPE &&
        isSmallScreen &&
        isSidebarVisible
      ) {
        event.preventDefault();
        event.stopPropagation();
        setSidebarVisible(false);
      }
    },
    [isSidebarVisible, isSmallScreen, setSidebarVisible]
  );

  const handleWindowResize = useCallback(() => {
    setSidebarPosition(isSidebarVisible, 'none');
  }, [isSidebarVisible, setSidebarPosition]);

  const handleTouchStart = useCallback(
    (event: TouchEvent) => {
      const touches = event.touches;

      if (touches.length !== 1) {
        return;
      }

      const x = touches[0].pageX;
      const y = touches[0].pageY;
      const width = getMobileSidebarWidth();

      if (isSidebarVisible && x > width) {
        return;
      }

      if (!isSidebarVisible && x > MOBILE_EDGE_SWIPE_WIDTH) {
        return;
      }

      touchStartX.current = x;
      touchStartY.current = y;
    },
    [isSidebarVisible]
  );

  const handleTouchMove = useCallback(
    (event: TouchEvent) => {
      if (touchStartX.current == null || touchStartY.current == null) {
        return;
      }

      const currentTouchX = event.touches[0].pageX;
      const currentTouchY = event.touches[0].pageY;
      const horizontalDistance = Math.abs(
        touchStartX.current - currentTouchX
      );
      const verticalDistance = Math.abs(touchStartY.current - currentTouchY);

      if (verticalDistance > horizontalDistance || horizontalDistance < 12) {
        return;
      }

      const width = getMobileSidebarWidth();
      const transform = isSidebarVisible
        ? Math.min(
            Math.max(currentTouchX - touchStartX.current, width * -1),
            0
          )
        : Math.min(currentTouchX - width, 0);

      setSidebarTransform({
        transition: 'none',
        transform,
      });
    },
    [isSidebarVisible]
  );

  const handleTouchEnd = useCallback(
    (event: TouchEvent) => {
      if (touchStartX.current == null) {
        return;
      }

      const currentTouchX = event.changedTouches[0].pageX;
      const travel = currentTouchX - touchStartX.current;
      const width = getMobileSidebarWidth();
      const threshold = width * 0.28;
      const shouldOpen = isSidebarVisible
        ? travel > threshold * -1
        : travel > threshold;

      setSidebarVisible(shouldOpen);
      setSidebarTransform({
        transition: 'transform 200ms ease-out',
        transform: shouldOpen ? 0 : width * -1,
      });
      touchStartX.current = null;
      touchStartY.current = null;
    },
    [isSidebarVisible, setSidebarVisible]
  );

  const handleTouchCancel = useCallback(() => {
    setSidebarPosition(isSidebarVisible);
    touchStartX.current = null;
    touchStartY.current = null;
  }, [isSidebarVisible, setSidebarPosition]);

  useEffect(() => {
    if (isSmallScreen) {
      window.addEventListener('keydown', handleKeyDown);
      window.addEventListener('resize', handleWindowResize);
      window.addEventListener('touchstart', handleTouchStart, {
        passive: true,
      });
      window.addEventListener('touchmove', handleTouchMove, {
        passive: true,
      });
      window.addEventListener('touchend', handleTouchEnd);
      window.addEventListener('touchcancel', handleTouchCancel);
    }

    return () => {
      window.removeEventListener('keydown', handleKeyDown);
      window.removeEventListener('resize', handleWindowResize);
      window.removeEventListener('touchstart', handleTouchStart);
      window.removeEventListener('touchmove', handleTouchMove);
      window.removeEventListener('touchend', handleTouchEnd);
      window.removeEventListener('touchcancel', handleTouchCancel);
    };
  }, [
    isSmallScreen,
    handleKeyDown,
    handleWindowResize,
    handleTouchStart,
    handleTouchMove,
    handleTouchEnd,
    handleTouchCancel,
  ]);

  useEffect(() => {
    setSidebarPosition(
      isSidebarVisible,
      isSmallScreen ? 'transform 200ms ease-out' : 'none'
    );
  }, [isSidebarVisible, isSmallScreen, setSidebarPosition]);

  useEffect(() => {
    const hasRouteChanged = previousPathname.current !== pathname;
    previousPathname.current = pathname;

    if (hasRouteChanged && isSmallScreen && isSidebarVisible) {
      setSidebarVisible(false);
    }
  }, [pathname, isSidebarVisible, isSmallScreen, setSidebarVisible]);

  useEffect(() => {
    const shouldLock = isSmallScreen && isSidebarVisible;

    elementClass(document.body)[shouldLock ? 'add' : 'remove'](
      styles.sidebarOpen
    );
    setScrollLock(shouldLock);

    return () => {
      elementClass(document.body).remove(styles.sidebarOpen);
      setScrollLock(false);
    };
  }, [isSidebarVisible, isSmallScreen]);

  useEffect(() => {
    const isMobileOpen = isSmallScreen && isSidebarVisible;
    const wasMobileOpen = previousMobileOpen.current;
    previousMobileOpen.current = isMobileOpen;

    if (isMobileOpen === wasMobileOpen) {
      return;
    }

    focusFrame.current = window.requestAnimationFrame(() => {
      const focusTarget = isMobileOpen
        ? sidebarRef.current?.querySelector<HTMLElement>(
            '[data-sidebar-close-button]'
          )
        : document.getElementById('sidebar-toggle-button');

      focusTarget?.focus({ preventScroll: true });
      focusFrame.current = null;
    });

    return () => {
      if (focusFrame.current != null) {
        window.cancelAnimationFrame(focusFrame.current);
        focusFrame.current = null;
      }
    };
  }, [isSidebarVisible, isSmallScreen]);

  const containerStyle = useMemo<SidebarContainerStyle | undefined>(() => {
    if (!isSmallScreen) {
      return undefined;
    }

    return {
      '--mobileSidebarWidth': `${sidebarWidth}px`,
      transition: sidebarTransform.transition,
      transform: `translateX(${sidebarTransform.transform}px)`,
    };
  }, [isSmallScreen, sidebarTransform, sidebarWidth]);

  const ScrollerComponent = isSmallScreen ? Scroller : OverlayScroller;
  const isSidebarFullyClosed =
    isSmallScreen &&
    !isSidebarVisible &&
    sidebarTransform.transform <= sidebarWidth * -1;

  const sidebar = (
    <aside
      id="primary-navigation"
      ref={sidebarRef}
      className={classNames(
        styles.sidebarContainer,
        isSmallScreen && styles.mobileSidebarContainer,
        isSmallScreen &&
          !isSidebarFullyClosed &&
          styles.sidebarContainerOpen,
        isSidebarFullyClosed && styles.sidebarContainerClosed
      )}
      style={containerStyle}
      aria-label={translate('Menu')}
      aria-hidden={isSidebarFullyClosed ? true : undefined}
      role={isSmallScreen && isSidebarVisible ? 'dialog' : undefined}
      aria-modal={isSmallScreen && isSidebarVisible ? true : undefined}
    >
      {isSmallScreen ? (
        <div className={styles.mobileHeader}>
          <span className={styles.mobileTitle}>{translate('Menu')}</span>

          <IconButton
            className={styles.closeButton}
            data-sidebar-close-button={true}
            name={icons.CLOSE}
            size={18}
            aria-label={translate('Close')}
            onPress={handleClose}
          />
        </div>
      ) : null}

      <ScrollerComponent
        className={styles.sidebar}
        scrollDirection="vertical"
      >
        <nav aria-label={translate('MainNavigation')}>
          {LINKS.map((link) => {
            const childWithStatusComponent = link.children?.find((child) => {
              return !!child.statusComponent;
            });

            const childStatusComponent = childWithStatusComponent
              ? childWithStatusComponent.statusComponent
              : null;

            const isActiveParent = activeParent === link.to;
            const hasActiveChild = hasActiveChildLink(link, pathname);

            return (
              <PageSidebarItem
                key={link.to}
                iconName={link.iconName}
                title={link.title}
                to={link.to}
                statusComponent={
                  isActiveParent || !childStatusComponent
                    ? link.statusComponent
                    : childStatusComponent
                }
                isActive={pathname === link.to && !hasActiveChild}
                isActiveParent={isActiveParent}
                isParentItem={!!link.children}
                onPress={handleItemPress}
              >
                {link.children &&
                  link.to === activeParent &&
                  link.children.map((child) => {
                    return (
                      <PageSidebarItem
                        key={child.to}
                        title={child.title}
                        to={child.to}
                        isActive={pathname === child.to}
                        isParentItem={false}
                        isChildItem={true}
                        statusComponent={child.statusComponent}
                        onPress={handleItemPress}
                      />
                    );
                  })}
              </PageSidebarItem>
            );
          })}
        </nav>

        <Messages />
      </ScrollerComponent>
    </aside>
  );

  if (!isSmallScreen) {
    return sidebar;
  }

  return (
    <>
      <button
        className={classNames(
          styles.backdrop,
          isSidebarVisible && styles.backdropOpen
        )}
        type="button"
        aria-label={translate('Close')}
        aria-hidden={!isSidebarVisible}
        tabIndex={-1}
        onClick={handleClose}
      />

      <FocusLock
        className={styles.focusLock}
        disabled={!isSidebarVisible}
        returnFocus={false}
      >
        {sidebar}
      </FocusLock>
    </>
  );
}

export default PageSidebar;
