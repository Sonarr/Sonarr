import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import classNames from 'classnames';
import { icons } from 'Helpers/Props';
import locationShape from 'Helpers/Props/Shapes/locationShape';
import dimensions from 'Styles/Variables/dimensions';
import OverlayScroller from 'Components/Scroller/OverlayScroller';
import Scroller from 'Components/Scroller/Scroller';
import QueueStatusConnector from 'Activity/Queue/Status/QueueStatusConnector';
import HealthStatusConnector from 'System/Status/Health/HealthStatusConnector';
import MessagesConnector from './Messages/MessagesConnector';
import PageSidebarItem from './PageSidebarItem';
import styles from './PageSidebar.css';

const HEADER_HEIGHT = parseInt(dimensions.headerHeight);
const SIDEBAR_WIDTH = parseInt(dimensions.sidebarWidth);

const links = [
  {
    iconName: icons.SERIES_CONTINUING,
    title: 'Series',
    to: '/',
    alias: '/series',
    children: [
      {
        title: 'Add New',
        to: '/add/new'
      },
      {
        title: 'Import',
        to: '/add/import'
      },
      {
        title: 'Mass Editor',
        to: '/serieseditor'
      },
      {
        title: 'Season Pass',
        to: '/seasonpass'
      }
    ]
  },

  {
    iconName: icons.CALENDAR,
    title: 'Calendar',
    to: '/calendar'
  },

  {
    iconName: icons.ACTIVITY,
    title: 'Activity',
    to: '/activity/queue',
    children: [
      {
        title: 'Queue',
        to: '/activity/queue',
        statusComponent: QueueStatusConnector
      },
      {
        title: 'History',
        to: '/activity/history'
      },
      {
        title: 'Blacklist',
        to: '/activity/blacklist'
      }
    ]
  },

  {
    iconName: icons.WARNING,
    title: 'Wanted',
    to: '/wanted/missing',
    children: [
      {
        title: 'Missing',
        to: '/wanted/missing'
      },
      {
        title: 'Cutoff Unmet',
        to: '/wanted/cutoffunmet'
      }
    ]
  },

  {
    iconName: icons.SETTINGS,
    title: 'Settings',
    to: '/settings',
    children: [
      {
        title: 'Media Management',
        to: '/settings/mediamanagement'
      },
      {
        title: 'Profiles',
        to: '/settings/profiles'
      },
      {
        title: 'Quality',
        to: '/settings/quality'
      },
      {
        title: 'Indexers',
        to: '/settings/indexers'
      },
      {
        title: 'Download Clients',
        to: '/settings/downloadclients'
      },
      {
        title: 'Connect',
        to: '/settings/connect'
      },
      {
        title: 'Metadata',
        to: '/settings/metadata'
      },
      {
        title: 'Tags',
        to: '/settings/tags'
      },
      {
        title: 'General',
        to: '/settings/general'
      },
      {
        title: 'UI',
        to: '/settings/ui'
      }
    ]
  },

  {
    iconName: icons.SYSTEM,
    title: 'System',
    to: '/system/status',
    children: [
      {
        title: 'Status',
        to: '/system/status',
        statusComponent: HealthStatusConnector
      },
      {
        title: 'Tasks',
        to: '/system/tasks'
      },
      {
        title: 'Backup',
        to: '/system/backup'
      },
      {
        title: 'Updates',
        to: '/system/updates'
      },
      {
        title: 'Events',
        to: '/system/events'
      },
      {
        title: 'Log Files',
        to: '/system/logs/files'
      }
    ]
  }
];

function getActiveParent(pathname) {
  let activeParent = links[0].to;

  links.forEach((link) => {
    if (link.to && link.to === pathname) {
      activeParent = link.to;

      return false;
    }

    const children = link.children;

    if (children) {
      children.forEach((childLink) => {
        if (pathname.startsWith(childLink.to)) {
          activeParent = link.to;

          return false;
        }
      });
    }

    if (
      (link.to !== '/' && pathname.startsWith(link.to)) ||
      (link.alias && pathname.startsWith(link.alias))
    ) {
      activeParent = link.to;

      return false;
    }
  });

  return activeParent;
}

function hasActiveChildLink(link, pathname) {
  const children = link.children;

  if (!children || !children.length) {
    return false;
  }

  return _.some(children, (child) => {
    return child.to === pathname;
  });
}

function getPositioning() {
  const windowScroll = window.scrollY == null ? document.documentElement.scrollTop : window.scrollY;
  const top = Math.max(HEADER_HEIGHT - windowScroll, 0);
  const height = window.innerHeight - top;

  return {
    top: `${top}px`,
    height: `${height}px`
  };
}

class PageSidebar extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._touchStartX = null;
    this._touchStartY = null;
    this._sidebarRef = null;

    this.state = {
      top: dimensions.headerHeight,
      height: `${window.innerHeight - HEADER_HEIGHT}px`,
      transition: null,
      transform: props.isSidebarVisible ? 0 : SIDEBAR_WIDTH * -1
    };
  }

  componentDidMount() {
    if (this.props.isSmallScreen) {
      window.addEventListener('click', this.onWindowClick, { capture: true });
      window.addEventListener('scroll', this.onWindowScroll);
      window.addEventListener('touchstart', this.onTouchStart);
      window.addEventListener('touchmove', this.onTouchMove);
      window.addEventListener('touchend', this.onTouchEnd);
      window.addEventListener('touchcancel', this.onTouchCancel);
    }
  }

  componentDidUpdate(prevProps) {
    const {
      isSidebarVisible
    } = this.props;

    const transform = this.state.transform;

    if (prevProps.isSidebarVisible !== isSidebarVisible) {
      this._setSidebarTransform(isSidebarVisible);
    } else if (transform === 0 && !isSidebarVisible) {
      this.props.onSidebarVisibleChange(true);
    } else if (transform === -SIDEBAR_WIDTH && isSidebarVisible) {
      this.props.onSidebarVisibleChange(false);
    }
  }

  componentWillUnmount() {
    if (this.props.isSmallScreen) {
      window.removeEventListener('click', this.onWindowClick, { capture: true });
      window.removeEventListener('scroll', this.onWindowScroll);
      window.removeEventListener('touchstart', this.onTouchStart);
      window.removeEventListener('touchmove', this.onTouchMove);
      window.removeEventListener('touchend', this.onTouchEnd);
      window.removeEventListener('touchcancel', this.onTouchCancel);
    }
  }

  //
  // Control

  _setSidebarRef = (ref) => {
    this._sidebarRef = ref;
  }

  _setSidebarTransform(isSidebarVisible, transition, callback) {
    this.setState({
      transition,
      transform: isSidebarVisible ? 0 : SIDEBAR_WIDTH * -1
    }, callback);
  }

  //
  // Listeners

  onWindowClick = (event) => {
    const sidebar = ReactDOM.findDOMNode(this._sidebarRef);
    const toggleButton = document.getElementById('sidebar-toggle-button');

    if (!sidebar) {
      return;
    }

    if (
      !sidebar.contains(event.target) &&
      !toggleButton.contains(event.target) &&
      this.props.isSidebarVisible
    ) {
      event.preventDefault();
      event.stopPropagation();
      this.props.onSidebarVisibleChange(false);
    }
  }

  onWindowScroll = () => {
    this.setState(getPositioning());
  }

  onTouchStart = (event) => {
    const touches = event.touches;
    const touchStartX = touches[0].pageX;
    const touchStartY = touches[0].pageY;
    const isSidebarVisible = this.props.isSidebarVisible;

    if (touches.length !== 1) {
      return;
    }

    if (isSidebarVisible && (touchStartX > 210 || touchStartX < 180)) {
      return;
    } else if (!isSidebarVisible && touchStartX > 40) {
      return;
    }

    this._touchStartX = touchStartX;
    this._touchStartY = touchStartY;
  }

  onTouchMove = (event) => {
    const touches = event.touches;
    const currentTouchX = touches[0].pageX;
    // const currentTouchY = touches[0].pageY;
    // const isSidebarVisible = this.props.isSidebarVisible;

    if (!this._touchStartX) {
      return;
    }

    // This is a bit funky when trying to close and you scroll
    // vertical too much by mistake, commenting out for now.
    // TODO: Evaluate if this should be nuked

    // if (Math.abs(this._touchStartY - currentTouchY) > 40) {
    //   const transform = isSidebarVisible ? 0 : SIDEBAR_WIDTH * -1;

    //   this.setState({
    //     transition: 'none',
    //     transform
    //   });

    //   return;
    // }

    if (Math.abs(this._touchStartX - currentTouchX) < 40) {
      return;
    }

    const transform = Math.min(currentTouchX - SIDEBAR_WIDTH, 0);

    this.setState({
      transition: 'none',
      transform
    });
  }

  onTouchEnd = (event) => {
    const touches = event.changedTouches;
    const currentTouch = touches[0].pageX;

    if (!this._touchStartX) {
      return;
    }

    if (currentTouch > this._touchStartX && currentTouch > 50) {
      this._setSidebarTransform(true, 'none');
    } else if (currentTouch < this._touchStartX && currentTouch < 80) {
      this._setSidebarTransform(false, 'transform 50ms ease-in-out');
    } else {
      this._setSidebarTransform(this.props.isSidebarVisible);
    }

    this._touchStartX = null;
    this._touchStartY = null;
  }

  onTouchCancel = (event) => {
    this._touchStartX = null;
    this._touchStartY = null;
  }

  onItemPress = () => {
    this.props.onSidebarVisibleChange(false);
  }

  //
  // Render

  render() {
    const {
      location,
      isSmallScreen
    } = this.props;

    const {
      top,
      height,
      transition,
      transform
    } = this.state;

    const urlBase = window.Sonarr.urlBase;
    const pathname = urlBase ? location.pathname.substr(urlBase.length) || '/' : location.pathname;
    const activeParent = getActiveParent(pathname);

    let containerStyle = {};
    let sidebarStyle = {};

    if (isSmallScreen) {
      containerStyle = {
        transition,
        transform: `translateX(${transform}px)`
      };

      sidebarStyle = {
        top,
        height
      };
    }

    const ScrollerComponent = isSmallScreen ? Scroller : OverlayScroller;

    return (
      <div
        ref={this._setSidebarRef}
        className={classNames(
          styles.sidebarContainer
        )}
        style={containerStyle}
      >
        <ScrollerComponent
          className={styles.sidebar}
          style={sidebarStyle}
        >
          <div>
            {
              links.map((link) => {
                const childWithStatusComponent = _.find(link.children, (child) => {
                  return !!child.statusComponent;
                });

                const childStatusComponent = childWithStatusComponent ?
                  childWithStatusComponent.statusComponent :
                  null;

                const isActiveParent = activeParent === link.to;
                const hasActiveChild = hasActiveChildLink(link, pathname);

                return (
                  <PageSidebarItem
                    key={link.to}
                    iconName={link.iconName}
                    title={link.title}
                    to={link.to}
                    statusComponent={isActiveParent || !childStatusComponent ? link.statusComponent : childStatusComponent}
                    isActive={pathname === link.to && !hasActiveChild}
                    isActiveParent={isActiveParent}
                    isParentItem={!!link.children}
                    onPress={this.onItemPress}
                  >
                    {
                      link.children && link.to === activeParent &&
                        link.children.map((child) => {
                          return (
                            <PageSidebarItem
                              key={child.to}
                              title={child.title}
                              to={child.to}
                              isActive={pathname.startsWith(child.to)}
                              isParentItem={false}
                              isChildItem={true}
                              statusComponent={child.statusComponent}
                              onPress={this.onItemPress}
                            />
                          );
                        })
                    }
                  </PageSidebarItem>
                );
              })
            }
          </div>

          <MessagesConnector />
        </ScrollerComponent>
      </div>
    );
  }
}

PageSidebar.propTypes = {
  location: locationShape.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  isSidebarVisible: PropTypes.bool.isRequired,
  onSidebarVisibleChange: PropTypes.func.isRequired
};

export default PageSidebar;
