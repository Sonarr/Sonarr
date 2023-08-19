import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import SpinnerButton from 'Components/Link/SpinnerButton';
import { icons, kinds } from 'Helpers/Props';
import styles from './SpinnerErrorButton.css';

function getTestResult(error) {
  if (!error) {
    return {
      wasSuccessful: true,
      hasWarning: false,
      hasError: false
    };
  }

  if (error.status !== 400) {
    return {
      wasSuccessful: false,
      hasWarning: false,
      hasError: true
    };
  }

  const failures = error.responseJSON;

  const hasWarning = _.some(failures, { isWarning: true });
  const hasError = _.some(failures, (failure) => !failure.isWarning);

  return {
    wasSuccessful: false,
    hasWarning,
    hasError
  };
}

class SpinnerErrorButton extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._testResultTimeout = null;

    this.state = {
      wasSuccessful: false,
      hasWarning: false,
      hasError: false
    };
  }

  componentDidUpdate(prevProps) {
    const {
      isSpinning,
      error
    } = this.props;

    if (prevProps.isSpinning && !isSpinning) {
      const testResult = getTestResult(error);

      this.setState(testResult, () => {
        const {
          wasSuccessful,
          hasWarning,
          hasError
        } = testResult;

        if (wasSuccessful || hasWarning || hasError) {
          this._testResultTimeout = setTimeout(this.resetState, 3000);
        }
      });
    }
  }

  componentWillUnmount() {
    if (this._testResultTimeout) {
      clearTimeout(this._testResultTimeout);
    }
  }

  //
  // Control

  resetState = () => {
    this.setState({
      wasSuccessful: false,
      hasWarning: false,
      hasError: false
    });
  };

  //
  // Render

  render() {
    const {
      kind,
      isSpinning,
      error,
      children,
      ...otherProps
    } = this.props;

    const {
      wasSuccessful,
      hasWarning,
      hasError
    } = this.state;

    const showIcon = wasSuccessful || hasWarning || hasError;

    let iconName = icons.CHECK;
    let iconKind = kind === kinds.PRIMARY ? kinds.DEFAULT : kinds.SUCCESS;

    if (hasWarning) {
      iconName = icons.WARNING;
      iconKind = kinds.WARNING;
    }

    if (hasError) {
      iconName = icons.DANGER;
      iconKind = kinds.DANGER;
    }

    return (
      <SpinnerButton
        kind={kind}
        isSpinning={isSpinning}
        {...otherProps}
      >
        <span className={showIcon ? styles.showIcon : undefined}>
          {
            showIcon &&
              <span className={styles.iconContainer}>
                <Icon
                  name={iconName}
                  kind={iconKind}
                />
              </span>
          }

          {
            <span className={styles.label}>
              {
                children
              }
            </span>
          }
        </span>
      </SpinnerButton>
    );
  }
}

SpinnerErrorButton.propTypes = {
  kind: PropTypes.oneOf(kinds.all),
  isSpinning: PropTypes.bool.isRequired,
  error: PropTypes.object,
  children: PropTypes.node.isRequired
};

export default SpinnerErrorButton;
