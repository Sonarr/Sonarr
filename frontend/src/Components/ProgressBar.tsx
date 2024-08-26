import classNames from 'classnames';
import React from 'react';
import { ColorImpairedConsumer } from 'App/ColorImpairedContext';
import { Kind } from 'Helpers/Props/kinds';
import { Size } from 'Helpers/Props/sizes';
import translate from 'Utilities/String/translate';
import styles from './ProgressBar.css';

interface ProgressBarProps {
  className?: string;
  containerClassName?: string;
  title?: string;
  progress: number;
  precision?: number;
  showText?: boolean;
  text?: string;
  kind?: Extract<Kind, keyof typeof styles>;
  size?: Extract<Size, keyof typeof styles>;
  width?: number;
}

function ProgressBar({
  className = styles.progressBar,
  containerClassName = styles.container,
  title,
  progress,
  precision = 1,
  showText = false,
  text,
  kind = 'primary',
  size = 'medium',
  width,
}: ProgressBarProps) {
  const progressPercent = `${progress.toFixed(precision)}%`;
  const progressText = text || progressPercent;
  const actualWidth = width ? `${width}px` : '100%';

  return (
    <ColorImpairedConsumer>
      {(enableColorImpairedMode) => {
        return (
          <div
            className={classNames(containerClassName, styles[size])}
            title={title}
            style={{ width: actualWidth }}
          >
            {showText && width ? (
              <div
                className={classNames(styles.backTextContainer, styles[kind])}
                style={{ width: actualWidth }}
              >
                <div className={styles.backText}>
                  <div>{progressText}</div>
                </div>
              </div>
            ) : null}

            <div
              className={classNames(
                className,
                styles[kind],
                enableColorImpairedMode && 'colorImpaired'
              )}
              role="meter"
              aria-label={translate('ProgressBarProgress', {
                progress: progress.toFixed(0),
              })}
              aria-valuenow={Math.floor(progress)}
              aria-valuemin={0}
              aria-valuemax={100}
              style={{ width: progressPercent }}
            />

            {showText ? (
              <div
                className={classNames(styles.frontTextContainer, styles[kind])}
                style={{ width: progressPercent }}
              >
                <div
                  className={styles.frontText}
                  style={{ width: actualWidth }}
                >
                  <div>{progressText}</div>
                </div>
              </div>
            ) : null}
          </div>
        );
      }}
    </ColorImpairedConsumer>
  );
}

export default ProgressBar;
