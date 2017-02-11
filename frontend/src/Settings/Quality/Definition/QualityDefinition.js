import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactSlider from 'react-slider';
import formatBytes from 'Utilities/Number/formatBytes';
import { kinds } from 'Helpers/Props';
import Label from 'Components/Label';
import NumberInput from 'Components/Form/NumberInput';
import TextInput from 'Components/Form/TextInput';
import styles from './QualityDefinition.css';

const slider = {
  min: 0,
  max: 200,
  step: 0.1
};

function getValue(value) {
  if (value < slider.min) {
    return slider.min;
  }

  if (value > slider.max) {
    return slider.max;
  }

  return value;
}

class QualityDefinition extends Component {

  //
  // Listeners

  onSizeChange = ([minSize, maxSize]) => {
    maxSize = maxSize === slider.max ? null : maxSize;

    this.props.onSizeChange({ minSize, maxSize });
  }

  onMinSizeChange = ({ value }) => {
    const minSize = getValue(value);

    this.props.onSizeChange({
      minSize,
      maxSize: this.props.maxSize
    });
  }

  onMaxSizeChange = ({ value }) => {
    const maxSize = value === slider.max ? null : getValue(value);

    this.props.onSizeChange({
      minSize: this.props.minSize,
      maxSize
    });
  }

  //
  // Render

  render() {
    const {
      id,
      quality,
      title,
      minSize,
      maxSize,
      advancedSettings,
      onTitleChange
    } = this.props;

    const minBytes = minSize * 1024 * 1024;
    const minThirty = formatBytes(minBytes * 30, 2);
    const minSixty = formatBytes(minBytes * 60, 2);

    const maxBytes = maxSize && maxSize * 1024 * 1024;
    const maxThirty = maxBytes ? formatBytes(maxBytes * 30, 2) : 'Unlimited';
    const maxSixty = maxBytes ? formatBytes(maxBytes * 60, 2) : 'Unlimited';

    return (
      <div className={styles.qualityDefinition}>
        <div className={styles.quality}>
          {quality.name}
        </div>

        <div className={styles.title}>
          <TextInput
            name={`${id}.${title}`}
            value={title}
            onChange={onTitleChange}
          />
        </div>

        <div className={styles.sizeLimit}>
          <ReactSlider
            min={slider.min}
            max={slider.max}
            step={slider.step}
            minDistance={10}
            value={[minSize || slider.min, maxSize || slider.max]}
            withBars={true}
            snapDragDisabled={true}
            className={styles.slider}
            barClassName={styles.bar}
            handleClassName={styles.handle}
            onChange={this.onSizeChange}
          />

          <div className={styles.sizes}>
            <div>
              <Label kind={kinds.WARNING}>{minThirty}</Label>
              <Label kind={kinds.INFO}>{minSixty}</Label>
            </div>

            <div>
              <Label kind={kinds.WARNING}>{maxThirty}</Label>
              <Label kind={kinds.INFO}>{maxSixty}</Label>
            </div>
          </div>
        </div>

        {
          advancedSettings &&
            <div className={styles.megabytesPerMinute}>
              <div>
                Min

                <NumberInput
                  className={styles.sizeInput}
                  name={`${id}.min`}
                  value={minSize || slider.min}
                  isFloat={true}
                  onChange={this.onMinSizeChange}
                />
              </div>

              <div>
                Max

                <NumberInput
                  className={styles.sizeInput}
                  name={`${id}.min`}
                  value={maxSize || slider.max}
                  isFloat={true}
                  onChange={this.onMaxSizeChange}
                />
              </div>
            </div>
        }
      </div>
    );
  }
}

QualityDefinition.propTypes = {
  id: PropTypes.number.isRequired,
  quality: PropTypes.object.isRequired,
  title: PropTypes.string.isRequired,
  minSize: PropTypes.number,
  maxSize: PropTypes.number,
  advancedSettings: PropTypes.bool.isRequired,
  onTitleChange: PropTypes.func.isRequired,
  onSizeChange: PropTypes.func.isRequired
};

export default QualityDefinition;
