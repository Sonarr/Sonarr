import PropTypes from 'prop-types';
import React, { Component } from 'react';
import colors from 'Styles/Variables/colors';
import styles from './CircularProgressBar.css';

class CircularProgressBar extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      progress: 0
    };
  }

  componentDidMount() {
    this._progressStep();
  }

  componentDidUpdate(prevProps) {
    const progress = this.props.progress;

    if (prevProps.progress !== progress) {
      this._cancelProgressStep();
      this._progressStep();
    }
  }

  componentWillUnmount() {
    this._cancelProgressStep();
  }

  //
  // Control

  _progressStep() {
    this.requestAnimationFrame = window.requestAnimationFrame(() => {
      this.setState({
        progress: this.state.progress + 1
      }, () => {
        if (this.state.progress < this.props.progress) {
          this._progressStep();
        }
      });
    });
  }

  _cancelProgressStep() {
    if (this.requestAnimationFrame) {
      window.cancelAnimationFrame(this.requestAnimationFrame);
    }
  }

  //
  // Render

  render() {
    const {
      className,
      containerClassName,
      size,
      strokeWidth,
      strokeColor,
      showProgressText
    } = this.props;

    const progress = this.state.progress;

    const center = size / 2;
    const radius = center - strokeWidth;
    const circumference = Math.PI * (radius * 2);
    const sizeInPixels = `${size}px`;
    const strokeDashoffset = ((100 - progress) / 100) * circumference;
    const progressText = `${Math.round(progress)}%`;

    return (
      <div
        className={containerClassName}
        style={{
          width: sizeInPixels,
          height: sizeInPixels,
          lineHeight: sizeInPixels
        }}
      >
        <svg
          className={className}
          version="1.1"
          xmlns="http://www.w3.org/2000/svg"
          width={size}
          height={size}
        >
          <circle
            fill="transparent"
            r={radius}
            cx={center}
            cy={center}
            strokeDasharray={circumference}
            style={{
              stroke: strokeColor,
              strokeWidth,
              strokeDashoffset
            }}
          />
        </svg>

        {
          showProgressText &&
            <div className={styles.circularProgressBarText}>
              {progressText}
            </div>
        }
      </div>
    );
  }
}

CircularProgressBar.propTypes = {
  className: PropTypes.string,
  containerClassName: PropTypes.string,
  size: PropTypes.number,
  progress: PropTypes.number.isRequired,
  strokeWidth: PropTypes.number,
  strokeColor: PropTypes.string,
  showProgressText: PropTypes.bool
};

CircularProgressBar.defaultProps = {
  className: styles.circularProgressBar,
  containerClassName: styles.circularProgressBarContainer,
  size: 60,
  strokeWidth: 5,
  strokeColor: colors.sonarrBlue,
  showProgressText: false
};

export default CircularProgressBar;
