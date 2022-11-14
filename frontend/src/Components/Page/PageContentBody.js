import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Scroller from 'Components/Scroller/Scroller';
import { scrollDirections } from 'Helpers/Props';
import { isLocked } from 'Utilities/scrollLock';
import styles from './PageContentBody.css';

class PageContentBody extends Component {

  //
  // Listeners

  onScroll = (props) => {
    const { onScroll } = this.props;

    if (this.props.onScroll && !isLocked()) {
      onScroll(props);
    }
  };

  //
  // Render

  render() {
    const {
      className,
      innerClassName,
      children,
      dispatch,
      ...otherProps
    } = this.props;

    return (
      <Scroller
        className={className}
        scrollDirection={scrollDirections.VERTICAL}
        {...otherProps}
        onScroll={this.onScroll}
      >
        <div className={innerClassName}>
          {children}
        </div>
      </Scroller>
    );
  }
}

PageContentBody.propTypes = {
  className: PropTypes.string,
  innerClassName: PropTypes.string,
  children: PropTypes.node.isRequired,
  onScroll: PropTypes.func,
  dispatch: PropTypes.func
};

PageContentBody.defaultProps = {
  className: styles.contentBody,
  innerClassName: styles.innerContentBody
};

export default PageContentBody;
