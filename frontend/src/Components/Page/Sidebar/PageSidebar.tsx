import classNames from 'classnames';
import React, {
  Fragment,
  useCallback,
  useEffect,
  useMemo,
  useRef,
} from 'react';
import ReactDOM from 'react-dom';
import { useLocation } from 'react-router';
import QueueStatus from 'Activity/Queue/Status/QueueStatus';
import {
  setIsSidebarVisible,
  useAppDimension,
  useAppValue,
} from 'App/appStore';
import { IconName } from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import OverlayScroller from 'Components/Scroller/OverlayScroller';
import Scroller from 'Components/Scroller/Scroller';
import { icons } from 'Helpers/Props';
import dimensions from 'Styles/Variables/dimensions';
import HealthStatus from 'System/Status/Health/HealthStatus';
import translate from 'Utilities/String/translate';
import Messages from './Messages/Messages';
import PageSidebarItem from './PageSidebarItem';
import styles from './PageSidebar.module.css';

const SIDEBAR_WIDTH = dimensions.sidebarWidth;

interface SidebarItem {
  iconName?: IconName;
  title: string | (() => string);
  to: string;
  alias?: string;
  isActive?: boolean;
  isActiveParent?: boolean;
  isParentItem?: boolean;
  isChildItem?: boolean;
  isHidden?: boolean;
  statusComponent?: React.ElementType;
  children?: {
    title: string | (() => string);
    to: string;
    statusComponent?: React.ElementType;
  }[];
}

interface SidebarGroup {
  label?: () => string;
  items: SidebarItem[];
}

const GROUPS: SidebarGroup[] = [
  {
    label: () => translate('Media'),
    items: [
      {
        iconName: icons.HOME,
        title: () => translate('Home'),
        to: '/',
        isHidden: true,
      },
      {
        iconName: icons.SERIES_CONTINUING,
        title: () => translate('Series'),
        to: '/series',
        alias: '/serieseditor',
        children: [
          {
            title: () => translate('AddNew'),
            to: '/add/new',
          },
          {
            title: () => translate('LibraryImport'),
            to: '/add/import',
          },
        ],
      },
      {
        iconName: icons.CALENDAR,
        title: () => translate('Calendar'),
        to: '/calendar',
      },
      {
        iconName: icons.STATISTICS,
        title: () => translate('Statistics'),
        to: '/statistics',
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
    ],
  },
  {
    label: () => translate('Activity'),
    items: [
      {
        iconName: icons.DOWNLOAD,
        title: () => translate('Queue'),
        to: '/activity/queue',
        statusComponent: QueueStatus,
      },
      {
        iconName: icons.HISTORY,
        title: () => translate('History'),
        to: '/activity/history',
      },
      {
        iconName: icons.BLOCKLIST,
        title: () => translate('Blocklist'),
        to: '/activity/blocklist',
      },
    ],
  },
  {
    items: [
      {
        iconName: icons.SETTINGS,
        title: () => translate('Settings'),
        to: '/settings',
        children: [
          {
            title: () => translate('MediaManagement'),
            to: '/settings/mediamanagement',
          },
          { title: () => translate('Profiles'), to: '/settings/profiles' },
          { title: () => translate('Quality'), to: '/settings/quality' },
          {
            title: () => translate('CustomFormats'),
            to: '/settings/customformats',
          },
          { title: () => translate('Indexers'), to: '/settings/indexers' },
          {
            title: () => translate('DownloadClients'),
            to: '/settings/downloadclients',
          },
          {
            title: () => translate('ImportLists'),
            to: '/settings/importlists',
          },
          { title: () => translate('Connect'), to: '/settings/connect' },
          { title: () => translate('Metadata'), to: '/settings/metadata' },
          {
            title: () => translate('MetadataSource'),
            to: '/settings/metadatasource',
          },
          { title: () => translate('Tags'), to: '/settings/tags' },
          { title: () => translate('General'), to: '/settings/general' },
          { title: () => translate('Ui'), to: '/settings/ui' },
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
          { title: () => translate('Tasks'), to: '/system/tasks' },
          { title: () => translate('Backups'), to: '/system/backups' },
          { title: () => translate('Updates'), to: '/system/updates' },
          { title: () => translate('Events'), to: '/system/events' },
          { title: () => translate('LogFiles'), to: '/system/logs/files' },
        ],
      },
    ],
  },
];

const FLAT_LINKS: SidebarItem[] = GROUPS.flatMap((g) => g.items).filter(
  (link) => !link.isHidden
);

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
  const isSidebarOverlay = useAppDimension('isSidebarOverlay');
  const isSidebarCollapsible = useAppDimension('isSidebarCollapsible');
  const location = useLocation();
  const sidebarRef = useRef(null);

  const { pathname } = location;

  const activeParent = useMemo(() => {
    return (
      FLAT_LINKS.find((link) => {
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
      })?.to ?? FLAT_LINKS[0].to
    );
  }, [pathname]);

  const handleWindowClick = useCallback(
    (event: MouseEvent) => {
      const sidebar = ReactDOM.findDOMNode(sidebarRef.current);
      const toggleButton = document.getElementById('sidebar-toggle-button');
      const target = event.target;

      if (!sidebar) {
        return;
      }

      if (
        target instanceof Node &&
        !sidebar.contains(target) &&
        !toggleButton?.contains(target) &&
        isSidebarVisible
      ) {
        event.preventDefault();
        event.stopPropagation();
        setIsSidebarVisible({ isSidebarVisible: false });
      }
    },
    [isSidebarVisible]
  );

  const handleItemPress = useCallback(() => {
    if (isSidebarOverlay) {
      setIsSidebarVisible({ isSidebarVisible: false });
    }
  }, [isSidebarOverlay]);

  const handleSidebarClosePress = useCallback(() => {
    setIsSidebarVisible({ isSidebarVisible: false });
  }, []);

  useEffect(() => {
    if (isSidebarOverlay) {
      window.addEventListener('click', handleWindowClick, { capture: true });
    }

    return () => {
      window.removeEventListener('click', handleWindowClick, { capture: true });
    };
  }, [isSidebarOverlay, handleWindowClick]);

  const containerStyle = useMemo(() => {
    if (!isSidebarOverlay) {
      return undefined;
    }

    return {
      transform: `translateX(${isSidebarVisible ? 0 : SIDEBAR_WIDTH * -1}px)`,
    };
  }, [isSidebarOverlay, isSidebarVisible]);

  const ScrollerComponent = isSidebarOverlay ? Scroller : OverlayScroller;

  return (
    <nav
      ref={sidebarRef}
      className={classNames(
        styles.sidebarContainer,
        isSidebarCollapsible &&
          !isSidebarOverlay &&
          !isSidebarVisible &&
          styles.isCollapsed
      )}
      style={containerStyle}
      aria-label={translate('MainNavigation')}
    >
      {isSidebarOverlay ? (
        <div className={styles.sidebarHeader}>
          <div className={styles.logoContainer}>
            <Link className={styles.logoLink} to="/">
              <img
                className={styles.logo}
                src={`${window.Sonarr.urlBase}/Content/Images/logo.svg`}
                alt=""
              />
              <span className={styles.brandName}>Sonarr</span>
            </Link>
          </div>

          <IconButton
            className={styles.sidebarCloseButton}
            name={icons.CLOSE}
            aria-label={translate('Close')}
            size={20}
            onPress={handleSidebarClosePress}
          />
        </div>
      ) : null}

      <ScrollerComponent
        className={styles.sidebar}
        scrollDirection="vertical"
        style={{
          height: `calc(${window.innerHeight}px - var(--headerHeight))`,
        }}
      >
        <div>
          {GROUPS.map((group, groupIndex) => {
            const showSeparatorBefore = !group.label && groupIndex > 0;

            return (
              <Fragment key={`group-${groupIndex}`}>
                {showSeparatorBefore ? (
                  <div className={styles.divider} aria-hidden="true" />
                ) : null}

                {group.label ? (
                  <span className={styles.groupLabel}>{group.label()}</span>
                ) : null}

                {group.items.map((link) => {
                  if (link.isHidden) {
                    return null;
                  }

                  const childWithStatusComponent = link.children?.find(
                    (child) => {
                      return !!child.statusComponent;
                    }
                  );

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
                      {link.children && link.to === activeParent
                        ? link.children.map((child) => {
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
                          })
                        : null}
                    </PageSidebarItem>
                  );
                })}
              </Fragment>
            );
          })}
        </div>

        <Messages />
      </ScrollerComponent>
    </nav>
  );
}

export default PageSidebar;
