import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactSlider from 'react-slider';
import NumberInput from 'Components/Form/NumberInput';
import TextInput from 'Components/Form/TextInput';
import Label from 'Components/Label';
import Popover from 'Components/Tooltip/Popover';
import { kinds, tooltipPositions } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';
import roundNumber from 'Utilities/Number/roundNumber';
import translate from 'Utilities/String/translate';
import QualityDefinitionLimits from './QualityDefinitionLimits';
import styles from './QualityDefinition.css';

const MIN = 0;
const MAX = 400;
const MIN_DISTANCE = 1;

const slider = {
  min: MIN,
  max: roundNumber(Math.pow(MAX, 1 / 1.1)),
  step: 0.1
};

function getValue(inputValue) {
  if (inputValue < MIN) {
    return MIN;
  }

  if (inputValue > MAX) {
    return MAX;
  }

  return roundNumber(inputValue);
}

function getSliderValue(value, defaultValue) {
  const sliderValue = value ? Math.pow(value, 1 / 1.1) : defaultValue;

  return roundNumber(sliderValue);
}

class QualityDefinition extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._forceUpdateTimeout = null;

    this.state = {
      sliderMinSize: getSliderValue(props.minSize, slider.min),
      sliderMaxSize: getSliderValue(props.maxSize, slider.max),
      sliderPreferredSize: getSliderValue(props.preferredSize, (slider.max - 3))
    };
  }

  componentDidMount() {
    // A hack to deal with a bug in the slider component until a fix for it
    // lands and an updated version is available.
    // See: https://github.com/mpowaga/react-slider/issues/115

    this._forceUpdateTimeout = setTimeout(() => this.forceUpdate(), 1);
  }

  componentWillUnmount() {
    if (this._forceUpdateTimeout) {
      clearTimeout(this._forceUpdateTimeout);
    }
  }

  //
  // Control

  trackRenderer(props, state) {
    return (
      <div
        {...props}
        className={styles.track}
      />
    );
  }

  thumbRenderer(props, state) {
    return (
      <div
        {...props}
        className={styles.thumb}
      />
    );
  }

  //
  // Listeners

  onSliderChange = ([sliderMinSize, sliderPreferredSize, sliderMaxSize]) => {
    this.setState({
      sliderMinSize,
      sliderMaxSize,
      sliderPreferredSize
    });

    this.props.onSizeChange({
      minSize: roundNumber(Math.pow(sliderMinSize, 1.1)),
      preferredSize: sliderPreferredSize === (slider.max - 3) ? null : roundNumber(Math.pow(sliderPreferredSize, 1.1)),
      maxSize: sliderMaxSize === slider.max ? null : roundNumber(Math.pow(sliderMaxSize, 1.1))
    });
  };

  onAfterSliderChange = () => {
    const {
      minSize,
      maxSize,
      preferredSize
    } = this.props;

    this.setState({
      sliderMiSize: getSliderValue(minSize, slider.min),
      sliderMaxSize: getSliderValue(maxSize, slider.max),
      sliderPreferredSize: getSliderValue(preferredSize, (slider.max - 3)) // fix
    });
  };

  onMinSizeChange = ({ value }) => {
    const minSize = getValue(value);

    this.setState({
      sliderMinSize: getSliderValue(minSize, slider.min)
    });

    this.props.onSizeChange({
      minSize,
      maxSize: this.props.maxSize,
      preferredSize: this.props.preferredSize
    });
  };

  onPreferredSizeChange = ({ value }) => {
    const preferredSize = value === (MAX - 3) ? null : getValue(value);

    this.setState({
      sliderPreferredSize: getSliderValue(preferredSize, slider.preferred)
    });

    this.props.onSizeChange({
      minSize: this.props.minSize,
      maxSize: this.props.maxSize,
      preferredSize
    });
  };

  onMaxSizeChange = ({ value }) => {
    const maxSize = value === MAX ? null : getValue(value);

    this.setState({
      sliderMaxSize: getSliderValue(maxSize, slider.max)
    });

    this.props.onSizeChange({
      minSize: this.props.minSize,
      maxSize,
      preferredSize: this.props.preferredSize
    });
  };

  //
  // Render

  render() {
    const {
      id,
      quality,
      title,
      minSize,
      maxSize,
      preferredSize,
      advancedSettings,
      onTitleChange
    } = this.props;

    const {
      sliderMinSize,
      sliderMaxSize,
      sliderPreferredSize
    } = this.state;

    const minBytes = minSize * 1024 * 1024;
    const minSixty = `${formatBytes(minBytes * 60)}/${translate('HourShorthand')}`;

    const preferredBytes = preferredSize * 1024 * 1024;
    const preferredSixty = preferredBytes ? `${formatBytes(preferredBytes * 60)}/${translate('HourShorthand')}` : translate('Unlimited');

    const maxBytes = maxSize && maxSize * 1024 * 1024;
    const maxSixty = maxBytes ? `${formatBytes(maxBytes * 60)}/${translate('HourShorthand')}` : translate('Unlimited');

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
            className={styles.slider}
            min={slider.min}
            max={slider.max}
            step={slider.step}
            minDistance={3}
            value={[sliderMinSize, sliderPreferredSize, sliderMaxSize]}
            withTracks={true}
            allowCross={false}
            snapDragDisabled={true}
            renderThumb={this.thumbRenderer}
            renderTrack={this.trackRenderer}
            onChange={this.onSliderChange}
            onAfterChange={this.onAfterSliderChange}
          />

          <div className={styles.sizes}>
            <div>
              <Popover
                anchor={
                  <Label kind={kinds.INFO}>{minSixty}</Label>
                }
                title={translate('MinimumLimits')}
                body={
                  <QualityDefinitionLimits
                    bytes={minBytes}
                    message={translate('NoMinimumForAnyRuntime')}
                  />
                }
                position={tooltipPositions.BOTTOM}
              />
            </div>

            <div>
              <Popover
                anchor={
                  <Label kind={kinds.SUCCESS}>{preferredSixty}</Label>
                }
                title={translate('PreferredSize')}
                body={
                  <QualityDefinitionLimits
                    bytes={preferredBytes}
                    message={translate('NoLimitForAnyRuntime')}
                  />
                }
                position={tooltipPositions.BOTTOM}
              />
            </div>

            <div>
              <Popover
                anchor={
                  <Label kind={kinds.WARNING}>{maxSixty}</Label>
                }
                title={translate('MaximumLimits')}
                body={
                  <QualityDefinitionLimits
                    bytes={maxBytes}
                    message={translate('NoLimitForAnyRuntime')}
                  />
                }
                position={tooltipPositions.BOTTOM}
              />
            </div>
          </div>
        </div>

        {
          advancedSettings &&
            <div className={styles.megabytesPerMinute}>
              <div>
                {translate('Min')}

                <NumberInput
                  className={styles.sizeInput}
                  name={`${id}.min`}
                  value={minSize || MIN}
                  min={MIN}
                  max={preferredSize ? preferredSize - 5 : MAX - 5}
                  step={0.1}
                  isFloat={true}
                  onChange={this.onMinSizeChange}
                />
              </div>

              <div>
                {translate('Preferred')}

                <NumberInput
                  className={styles.sizeInput}
                  name={`${id}.min`}
                  value={preferredSize || MAX - 5}
                  min={MIN}
                  max={maxSize ? maxSize - 5 : MAX - 5}
                  step={0.1}
                  isFloat={true}
                  onChange={this.onPreferredSizeChange}
                />
              </div>

              <div>
                {translate('Max')}

                <NumberInput
                  className={styles.sizeInput}
                  name={`${id}.max`}
                  value={maxSize || MAX}
                  min={minSize + MIN_DISTANCE}
                  max={MAX}
                  step={0.1}
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
  preferredSize: PropTypes.number,
  advancedSettings: PropTypes.bool.isRequired,
  onTitleChange: PropTypes.func.isRequired,
  onSizeChange: PropTypes.func.isRequired
};

export default QualityDefinition;
