import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import { ColorImpairedConsumer } from 'App/ColorImpairedContext';
import { kinds, sizes } from 'Helpers/Props';
import styles from './ProgressBar.css';

function ProgressBar(props) {
  const {
    className,
    containerClassName,
    title,
    progress,
    precision,
    showText,
    text,
    kind,
    size,
    width
  } = props;

  const progressPercent = `${progress.toFixed(precision)}%`;
  const progressText = text || progressPercent;
  const actualWidth = width ? `${width}px` : '100%';

  return (
    <ColorImpairedConsumer>
      {(enableColorImpairedMode) => {
        return (
          <div
            className={classNames(
              containerClassName,
              styles[size]
            )}
            title={title}
            style={{ width: actualWidth }}
          >
            {
              showText && width ?
                <div
                  className={styles.backTextContainer}
                  style={{ width: actualWidth }}
                >
                  <div className={styles.backText}>
                    <div>
                      {progressText}
                    </div>
                  </div>
                </div> :
                null
            }

            <div
              className={classNames(
                className,
                styles[kind],
                enableColorImpairedMode && 'colorImpaired'
              )}
              aria-valuenow={progress}
              aria-valuemin="0"
              aria-valuemax="100"
              style={{ width: progressPercent }}
            />

            {
              showText ?
                <div
                  className={styles.frontTextContainer}
                  style={{ width: progressPercent }}
                >
                  <div
                    className={styles.frontText}
                    style={{ width: actualWidth }}
                  >
                    <div>
                      {progressText}
                    </div>
                  </div>
                </div> :
                null
            }
          </div>
        );
      }}
    </ColorImpairedConsumer>
  );
}

ProgressBar.propTypes = {
  className: PropTypes.string,
  containerClassName: PropTypes.string,
  title: PropTypes.string,
  progress: PropTypes.number.isRequired,
  precision: PropTypes.number.isRequired,
  showText: PropTypes.bool.isRequired,
  text: PropTypes.string,
  kind: PropTypes.oneOf(kinds.all).isRequired,
  size: PropTypes.oneOf(sizes.all).isRequired,
  width: PropTypes.number
};

ProgressBar.defaultProps = {
  className: styles.progressBar,
  containerClassName: styles.container,
  precision: 1,
  showText: false,
  kind: kinds.PRIMARY,
  size: sizes.MEDIUM
};

export default ProgressBar;
