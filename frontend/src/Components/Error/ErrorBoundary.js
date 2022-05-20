import * as sentry from '@sentry/browser';
import PropTypes from 'prop-types';
import React, { Component } from 'react';

class ErrorBoundary extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      error: null,
      info: null
    };
  }

  componentDidCatch(error, info) {
    this.setState({
      error,
      info
    });

    sentry.captureException(error);
  }

  //
  // Render

  render() {
    const {
      children,
      errorComponent: ErrorComponent,
      ...otherProps
    } = this.props;

    const {
      error,
      info
    } = this.state;

    if (error) {
      return (
        <ErrorComponent
          error={error}
          info={info}
          {...otherProps}
        />
      );
    }

    return children;
  }
}

ErrorBoundary.propTypes = {
  children: PropTypes.node.isRequired,
  errorComponent: PropTypes.elementType.isRequired
};

export default ErrorBoundary;
