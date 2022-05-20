import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Measure from 'Components/Measure';
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
      visibleItems: props.items.order
    };
  }

  componentDidMount() {
    this.computeVisibleItems();
  }

  shouldComponentUpdate(nextProps, nextState) {
    return (
      nextProps.items !== this.props.items ||
      nextState.height !== this.state.height ||
      nextState.visibleItems !== this.state.visibleItems
    );
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

    if (!items) {
      return;
    }

    const {
      characters,
      order
    } = items;

    const height = this.state.height;
    const maximumItems = Math.floor(height / ITEM_HEIGHT);
    const diff = order.length - maximumItems;

    if (diff < 0) {
      this.setState({ visibleItems: order });
      return;
    }

    if (order.length < minimumItems) {
      this.setState({ visibleItems: order });
      return;
    }

    // get first, last, and most common in between to make up numbers
    const visibleItems = [order[0]];

    const sorted = order.slice(1, -1).map((x) => characters[x]).sort((a, b) => b - a);
    const minCount = sorted[maximumItems - 3];
    const greater = sorted.reduce((acc, value) => acc + (value > minCount ? 1 : 0), 0);
    let minAllowed = maximumItems - 2 - greater;

    for (let i = 1; i < order.length - 1; i++) {
      if (characters[order[i]] > minCount) {
        visibleItems.push(order[i]);
      } else if (characters[order[i]] === minCount && minAllowed > 0) {
        visibleItems.push(order[i]);
        minAllowed--;
      }
    }

    visibleItems.push(order[order.length - 1]);

    this.setState({ visibleItems });
  }

  //
  // Listeners

  onMeasure = ({ height }) => {
    if (height > 0) {
      this.setState({ height });
    }
  };

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
  items: PropTypes.object.isRequired,
  minimumItems: PropTypes.number.isRequired,
  onItemPress: PropTypes.func.isRequired
};

PageJumpBar.defaultProps = {
  minimumItems: 5
};

export default PageJumpBar;
