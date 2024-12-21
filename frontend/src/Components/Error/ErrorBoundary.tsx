import * as sentry from '@sentry/browser';
import React, { Component, ErrorInfo } from 'react';

interface ErrorBoundaryProps {
  children: React.ReactNode;
  errorComponent: React.ElementType;
  onModalClose?: () => void;
}

interface ErrorBoundaryState {
  error: Error | null;
  info: ErrorInfo | null;
}

// Class component until componentDidCatch is supported in functional components
class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);

    this.state = {
      error: null,
      info: null,
    };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    this.setState({
      error,
      info,
    });

    sentry.captureException(error);
  }

  render() {
    const {
      children,
      errorComponent: ErrorComponent,
      onModalClose,
    } = this.props;
    const { error, info } = this.state;

    if (error) {
      return (
        <ErrorComponent error={error} info={info} onModalClose={onModalClose} />
      );
    }

    return children;
  }
}

export default ErrorBoundary;
