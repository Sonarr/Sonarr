import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Measure from 'react-measure';
import dimensions from 'Styles/Variables/dimensions';
import PageJumpBarItem from './PageJumpBarItem';
import styles from './PageJumpBar.css';

const ITEM_HEIGHT = parseInt(dimensions.jumpBarItemHeight);

class PageJumpBar extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      height: 0,
      visibleItems: props.items
    };
  }

  componentDidMount() {
    this.computeVisibleItems();
  }

  componentDidUpdate(prevProps, prevState) {
    if (
      prevProps.items !== this.props.items ||
      prevState.height !== this.state.height
    ) {
      this.computeVisibleItems();
    }
  }

  //
  // Control

  computeVisibleItems() {
    const {
      items,
      minimumItems
    } = this.props;

    const height = this.state.height;
    const maximumItems = Math.floor(height / ITEM_HEIGHT);
    const diff = items.length - maximumItems;

    if (diff < 0) {
      this.setState({ visibleItems: items });
      return;
    }

    if (items.length < minimumItems) {
      this.setState({ visibleItems: items });
      return;
    }

    const removeDiff = Math.ceil(items.length / maximumItems);

    const visibleItems = _.reduce(items, (acc, item, index) => {
      if (index % removeDiff === 0) {
        acc.push(item);
      }

      return acc;
    }, []);

    this.setState({ visibleItems });
  }

  //
  // Listeners

  onMeasure = ({ height }) => {
    this.setState({ height });
  }

  //
  // Render

  render() {
    const {
      minimumItems,
      onItemPress
    } = this.props;

    const {
      visibleItems
    } = this.state;

    if (!visibleItems.length || visibleItems.length < minimumItems) {
      return null;
    }

    return (
      <div className={styles.jumpBar}>
        <Measure
          whitelist={['height']}
          onMeasure={this.onMeasure}
        >
          <div className={styles.jumpBarItems}>
            {
              visibleItems.map((item) => {
                return (
                  <PageJumpBarItem
                    key={item}
                    label={item}
                    onItemPress={onItemPress}
                  />
                );
              })
            }
          </div>
        </Measure>
      </div>
    );
  }
}

PageJumpBar.propTypes = {
  items: PropTypes.arrayOf(PropTypes.string).isRequired,
  minimumItems: PropTypes.number.isRequired,
  onItemPress: PropTypes.func.isRequired
};

PageJumpBar.defaultProps = {
  minimumItems: 5
};

export default PageJumpBar;
