import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { map } from 'Helpers/elementChildren';
import styles from './PageSidebarItem.css';

class PageSidebarItem extends Component {

  //
  // Listeners

  onPress = () => {
    const {
      isChildItem,
      isParentItem,
      onPress
    } = this.props;

    if (isChildItem || !isParentItem) {
      onPress();
    }
  };

  //
  // Render

  render() {
    const {
      iconName,
      title,
      to,
      isActive,
      isActiveParent,
      isChildItem,
      statusComponent: StatusComponent,
      children
    } = this.props;

    return (
      <div
        className={classNames(
          styles.item,
          isActiveParent && styles.isActiveItem
        )}
      >
        <Link
          className={classNames(
            isChildItem ? styles.childLink : styles.link,
            isActiveParent && styles.isActiveParentLink,
            isActive && styles.isActiveLink
          )}
          to={to}
          onPress={this.onPress}
        >
          {
            !!iconName &&
              <span className={styles.iconContainer}>
                <Icon
                  name={iconName}
                />
              </span>
          }

          <span className={isChildItem ? styles.noIcon : null}>
            {typeof title === 'function' ? title() : title}
          </span>

          {
            !!StatusComponent &&
              <span className={styles.status}>
                <StatusComponent />
              </span>
          }
        </Link>

        {
          children &&
            map(children, (child) => {
              return React.cloneElement(child, { isChildItem: true });
            })
        }
      </div>
    );
  }
}

PageSidebarItem.propTypes = {
  iconName: PropTypes.object,
  title: PropTypes.oneOfType([PropTypes.string, PropTypes.func]).isRequired,
  to: PropTypes.string.isRequired,
  isActive: PropTypes.bool,
  isActiveParent: PropTypes.bool,
  isParentItem: PropTypes.bool.isRequired,
  isChildItem: PropTypes.bool.isRequired,
  statusComponent: PropTypes.elementType,
  children: PropTypes.node,
  onPress: PropTypes.func
};

PageSidebarItem.defaultProps = {
  isChildItem: false
};

export default PageSidebarItem;
