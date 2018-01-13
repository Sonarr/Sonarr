import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import Measure from 'react-measure';
import TetherComponent from 'react-tether';
import classNames from 'classnames';
import isMobileUtil from 'Utilities/isMobile';
import * as keyCodes from 'Utilities/Constants/keyCodes';
import { icons, scrollDirections } from 'Helpers/Props';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import Scroller from 'Components/Scroller/Scroller';
import EnhancedSelectInputSelectedValue from './EnhancedSelectInputSelectedValue';
import EnhancedSelectInputOption from './EnhancedSelectInputOption';
import styles from './EnhancedSelectInput.css';

const tetherOptions = {
  skipMoveElement: true,
  constraints: [
    {
      to: 'window',
      attachment: 'together',
      pin: true
    }
  ],
  attachment: 'top left',
  targetAttachment: 'bottom left'
};

function isArrowKey(keyCode) {
  return keyCode === keyCodes.UP_ARROW || keyCode === keyCodes.DOWN_ARROW;
}

function getSelectedOption(selectedIndex, values) {
  return values[selectedIndex];
}

function findIndex(startingIndex, direction, values) {
  let indexToTest = startingIndex + direction;

  while (indexToTest !== startingIndex) {
    if (indexToTest < 0) {
      indexToTest = values.length - 1;
    } else if (indexToTest >= values.length) {
      indexToTest = 0;
    }

    if (getSelectedOption(indexToTest, values).isDisabled) {
      indexToTest = indexToTest + direction;
    } else {
      return indexToTest;
    }
  }
}

function previousIndex(selectedIndex, values) {
  return findIndex(selectedIndex, -1, values);
}

function nextIndex(selectedIndex, values) {
  return findIndex(selectedIndex, 1, values);
}

function getSelectedIndex(props) {
  const {
    value,
    values
  } = props;

  return values.findIndex((v) => {
    return v.key === value;
  });
}

function getKey(selectedIndex, values) {
  return values[selectedIndex].key;
}

class EnhancedSelectInput extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isOpen: false,
      selectedIndex: getSelectedIndex(props),
      width: 0,
      isMobile: isMobileUtil()
    };
  }

  componentDidUpdate(prevProps) {
    if (prevProps.value !== this.props.value) {
      this.setState({
        selectedIndex: getSelectedIndex(this.props)
      });
    }
  }

  //
  // Control

  _setButtonRef = (ref) => {
    this._buttonRef = ref;
  }

  _setOptionsRef = (ref) => {
    this._optionsRef = ref;
  }

  _addListener() {
    window.addEventListener('click', this.onWindowClick);
  }

  _removeListener() {
    window.removeEventListener('click', this.onWindowClick);
  }

  //
  // Listeners

  onWindowClick = (event) => {
    const button = ReactDOM.findDOMNode(this._buttonRef);
    const options = ReactDOM.findDOMNode(this._optionsRef);

    if (!button || this.state.isMobile) {
      return;
    }

    if (
      !button.contains(event.target) &&
      options &&
      !options.contains(event.target) &&
      this.state.isOpen
    ) {
      this.setState({ isOpen: false });
      this._removeListener();
    }
  }

  onBlur = () => {
    this.setState({
      selectedIndex: getSelectedIndex(this.props)
    });
  }

  onKeyDown = (event) => {
    const {
      values
    } = this.props;

    const {
      isOpen,
      selectedIndex
    } = this.state;

    const keyCode = event.keyCode;
    const newState = {};

    if (!isOpen) {
      if (isArrowKey(keyCode)) {
        event.preventDefault();
        newState.isOpen = true;
      }

      if (
        selectedIndex == null ||
        getSelectedOption(selectedIndex, values).isDisabled
      ) {
        if (keyCode === keyCodes.UP_ARROW) {
          newState.selectedIndex = previousIndex(0, values);
        } else if (keyCode === keyCodes.DOWN_ARROW) {
          newState.selectedIndex = nextIndex(values.length - 1, values);
        }
      }

      this.setState(newState);
      return;
    }

    if (keyCode === keyCodes.UP_ARROW) {
      event.preventDefault();
      newState.selectedIndex = previousIndex(selectedIndex, values);
    }

    if (keyCode === keyCodes.DOWN_ARROW) {
      event.preventDefault();
      newState.selectedIndex = nextIndex(selectedIndex, values);
    }

    if (keyCode === keyCodes.ENTER) {
      event.preventDefault();
      newState.isOpen = false;
      this.onSelect(getKey(selectedIndex, values));
    }

    if (keyCode === keyCodes.TAB) {
      newState.isOpen = false;
      this.onSelect(getKey(selectedIndex, values));
    }

    if (keyCode === keyCodes.ESCAPE) {
      event.preventDefault();
      event.stopPropagation();
      newState.isOpen = false;
      newState.selectedIndex = getSelectedIndex(this.props);
    }

    if (!_.isEmpty(newState)) {
      this.setState(newState);
    }
  }

  onPress = () => {
    if (this.state.isOpen) {
      this._removeListener();
    } else {
      this._addListener();
    }

    this.setState({ isOpen: !this.state.isOpen });
  }

  onSelect = (value) => {
    this.setState({ isOpen: false });

    this.props.onChange({
      name: this.props.name,
      value
    });
  }

  onMeasure = ({ width }) => {
    this.setState({ width });
  }

  onOptionsModalClose = () => {
    this.setState({ isOpen: false });
  }

  //
  // Render

  render() {
    const {
      className,
      disabledClassName,
      values,
      isDisabled,
      hasError,
      hasWarning,
      selectedValueOptions,
      selectedValueComponent: SelectedValueComponent,
      optionComponent: OptionComponent
    } = this.props;

    const {
      selectedIndex,
      width,
      isOpen,
      isMobile
    } = this.state;

    const selectedOption = getSelectedOption(selectedIndex, values);

    return (
      <div>
        <TetherComponent
          classes={{
            element: styles.tether
          }}
          {...tetherOptions}
        >
          <Measure
            whitelist={['width']}
            onMeasure={this.onMeasure}
          >
            <Link
              ref={this._setButtonRef}
              className={classNames(
                className,
                hasError && styles.hasError,
                hasWarning && styles.hasWarning,
                isDisabled && disabledClassName
              )}
              isDisabled={isDisabled}
              onBlur={this.onBlur}
              onKeyDown={this.onKeyDown}
              onPress={this.onPress}
            >
              <SelectedValueComponent
                {...selectedValueOptions}
                {...selectedOption}
                isDisabled={isDisabled}
              >
                {selectedOption ? selectedOption.value : null}
              </SelectedValueComponent>

              <div
                className={isDisabled ?
                  styles.dropdownArrowContainerDisabled :
                  styles.dropdownArrowContainer
                }
              >
                <Icon
                  name={icons.CARET_DOWN}
                />
              </div>
            </Link>
          </Measure>

          {
            isOpen && !isMobile &&
              <div
                ref={this._setOptionsRef}
                className={styles.optionsContainer}
                style={{
                  minWidth: width
                }}
              >
                <div className={styles.options}>
                  {
                    values.map((v, index) => {
                      return (
                        <OptionComponent
                          key={v.key}
                          id={v.key}
                          isSelected={index === selectedIndex}
                          {...v}
                          isMobile={false}
                          onSelect={this.onSelect}
                        >
                          {v.value}
                        </OptionComponent>
                      );
                    })
                  }
                </div>
              </div>
          }
        </TetherComponent>

        {
          isMobile &&
            <Modal
              className={styles.optionsModal}
              isOpen={isOpen}
              onModalClose={this.onOptionsModalClose}
            >
              <ModalBody
                className={styles.optionsModalBody}
                innerClassName={styles.optionsInnerModalBody}
                scrollDirection={scrollDirections.NONE}
              >
                <Scroller className={styles.optionsModalScroller}>
                  {
                    values.map((v, index) => {
                      return (
                        <OptionComponent
                          key={v.key}
                          id={v.key}
                          isSelected={index === selectedIndex}
                          {...v}
                          isMobile={true}
                          onSelect={this.onSelect}
                        >
                          {v.value}
                        </OptionComponent>
                      );
                    })
                  }
                </Scroller>
              </ModalBody>
            </Modal>
        }
      </div>
    );
  }
}

EnhancedSelectInput.propTypes = {
  className: PropTypes.string,
  disabledClassName: PropTypes.string,
  name: PropTypes.string.isRequired,
  value: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  values: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDisabled: PropTypes.bool,
  hasError: PropTypes.bool,
  hasWarning: PropTypes.bool,
  selectedValueOptions: PropTypes.object.isRequired,
  selectedValueComponent: PropTypes.oneOfType([PropTypes.string, PropTypes.func]).isRequired,
  optionComponent: PropTypes.func,
  onChange: PropTypes.func.isRequired
};

EnhancedSelectInput.defaultProps = {
  className: styles.enhancedSelect,
  disabledClassName: styles.isDisabled,
  isDisabled: false,
  selectedValueOptions: {},
  selectedValueComponent: EnhancedSelectInputSelectedValue,
  optionComponent: EnhancedSelectInputOption
};

export default EnhancedSelectInput;
