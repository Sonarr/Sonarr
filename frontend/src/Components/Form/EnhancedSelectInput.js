import classNames from 'classnames';
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Manager, Popper, Reference } from 'react-popper';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Measure from 'Components/Measure';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import Portal from 'Components/Portal';
import Scroller from 'Components/Scroller/Scroller';
import { icons, scrollDirections, sizes } from 'Helpers/Props';
import { isMobile as isMobileUtil } from 'Utilities/browser';
import * as keyCodes from 'Utilities/Constants/keyCodes';
import getUniqueElememtId from 'Utilities/getUniqueElementId';
import HintedSelectInputOption from './HintedSelectInputOption';
import HintedSelectInputSelectedValue from './HintedSelectInputSelectedValue';
import TextInput from './TextInput';
import styles from './EnhancedSelectInput.css';

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

  if (Array.isArray(value)) {
    return values.findIndex((v) => {
      return value.size && v.key === value[0];
    });
  }

  return values.findIndex((v) => {
    return v.key === value;
  });
}

function isSelectedItem(index, props) {
  const {
    value,
    values
  } = props;

  if (Array.isArray(value)) {
    return value.includes(values[index].key);
  }

  return values[index].key === value;
}

function getKey(selectedIndex, values) {
  return values[selectedIndex].key;
}

class EnhancedSelectInput extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._scheduleUpdate = null;
    this._buttonId = getUniqueElememtId();
    this._optionsId = getUniqueElememtId();

    this.state = {
      isOpen: false,
      selectedIndex: getSelectedIndex(props),
      width: 0,
      isMobile: isMobileUtil()
    };
  }

  componentDidUpdate(prevProps) {
    if (this._scheduleUpdate) {
      this._scheduleUpdate();
    }

    if (!Array.isArray(this.props.value) && prevProps.value !== this.props.value) {
      this.setState({
        selectedIndex: getSelectedIndex(this.props)
      });
    }
  }

  //
  // Control

  _addListener() {
    window.addEventListener('click', this.onWindowClick);
  }

  _removeListener() {
    window.removeEventListener('click', this.onWindowClick);
  }

  //
  // Listeners

  onComputeMaxHeight = (data) => {
    const {
      top,
      bottom
    } = data.offsets.reference;

    const windowHeight = window.innerHeight;

    if ((/^botton/).test(data.placement)) {
      data.styles.maxHeight = windowHeight - bottom;
    } else {
      data.styles.maxHeight = top;
    }

    return data;
  };

  onWindowClick = (event) => {
    const button = document.getElementById(this._buttonId);
    const options = document.getElementById(this._optionsId);

    if (!button || !event.target.isConnected || this.state.isMobile) {
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
  };

  onFocus = () => {
    if (this.state.isOpen) {
      this._removeListener();
      this.setState({ isOpen: false });
    }
  };

  onBlur = () => {
    if (!this.props.isEditable) {
      // Calling setState without this check prevents the click event from being properly handled on Chrome (it is on firefox)
      const origIndex = getSelectedIndex(this.props);

      if (origIndex !== this.state.selectedIndex) {
        this.setState({ selectedIndex: origIndex });
      }
    }
  };

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
        selectedIndex == null || selectedIndex === -1 ||
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
  };

  onPress = () => {
    if (this.state.isOpen) {
      this._removeListener();
    } else {
      this._addListener();
    }

    if (!this.state.isOpen && this.props.onOpen) {
      this.props.onOpen();
    }

    this.setState({ isOpen: !this.state.isOpen });
  };

  onSelect = (value) => {
    if (Array.isArray(this.props.value)) {
      let newValue = null;
      const index = this.props.value.indexOf(value);
      if (index === -1) {
        newValue = this.props.values.map((v) => v.key).filter((v) => (v === value) || this.props.value.includes(v));
      } else {
        newValue = [...this.props.value];
        newValue.splice(index, 1);
      }
      this.props.onChange({
        name: this.props.name,
        value: newValue
      });
    } else {
      this.setState({ isOpen: false });

      this.props.onChange({
        name: this.props.name,
        value
      });
    }
  };

  onMeasure = ({ width }) => {
    this.setState({ width });
  };

  onOptionsModalClose = () => {
    this.setState({ isOpen: false });
  };

  //
  // Render

  render() {
    const {
      className,
      disabledClassName,
      name,
      value,
      values,
      isDisabled,
      isEditable,
      isFetching,
      hasError,
      hasWarning,
      valueOptions,
      selectedValueOptions,
      selectedValueComponent: SelectedValueComponent,
      optionComponent: OptionComponent,
      onChange
    } = this.props;

    const {
      selectedIndex,
      width,
      isOpen,
      isMobile
    } = this.state;

    const isMultiSelect = Array.isArray(value);
    const selectedOption = getSelectedOption(selectedIndex, values);

    return (
      <div>
        <Manager>
          <Reference>
            {({ ref }) => (
              <div
                ref={ref}
                id={this._buttonId}
              >
                <Measure
                  whitelist={['width']}
                  onMeasure={this.onMeasure}
                >
                  {
                    isEditable ?
                      <div
                        className={styles.editableContainer}
                      >
                        <TextInput
                          className={className}
                          name={name}
                          value={value}
                          readOnly={isDisabled}
                          hasError={hasError}
                          hasWarning={hasWarning}
                          onFocus={this.onFocus}
                          onBlur={this.onBlur}
                          onChange={onChange}
                        />
                        <Link
                          className={classNames(
                            styles.dropdownArrowContainerEditable,
                            isDisabled ?
                              styles.dropdownArrowContainerDisabled :
                              styles.dropdownArrowContainer)
                          }
                          onPress={this.onPress}
                        >
                          {
                            isFetching &&
                              <LoadingIndicator
                                className={styles.loading}
                                size={20}
                              />
                          }

                          {
                            !isFetching &&
                              <Icon
                                name={icons.CARET_DOWN}
                              />
                          }
                        </Link>
                      </div> :
                      <Link
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
                          value={value}
                          values={values}
                          {...selectedValueOptions}
                          {...selectedOption}
                          isDisabled={isDisabled}
                          isMultiSelect={isMultiSelect}
                        >
                          {selectedOption ? selectedOption.value : null}
                        </SelectedValueComponent>

                        <div
                          className={isDisabled ?
                            styles.dropdownArrowContainerDisabled :
                            styles.dropdownArrowContainer
                          }
                        >

                          {
                            isFetching &&
                              <LoadingIndicator
                                className={styles.loading}
                                size={20}
                              />
                          }

                          {
                            !isFetching &&
                              <Icon
                                name={icons.CARET_DOWN}
                              />
                          }
                        </div>
                      </Link>
                  }
                </Measure>
              </div>
            )}
          </Reference>
          <Portal>
            <Popper
              placement="bottom-start"
              modifiers={{
                computeMaxHeight: {
                  order: 851,
                  enabled: true,
                  fn: this.onComputeMaxHeight
                }
              }}
            >
              {({ ref, style, scheduleUpdate }) => {
                this._scheduleUpdate = scheduleUpdate;

                return (
                  <div
                    ref={ref}
                    id={this._optionsId}
                    className={styles.optionsContainer}
                    style={{
                      ...style,
                      minWidth: width
                    }}
                  >
                    {
                      isOpen && !isMobile ?
                        <Scroller
                          className={styles.options}
                          style={{
                            maxHeight: style.maxHeight
                          }}
                        >
                          {
                            values.map((v, index) => {
                              const hasParent = v.parentKey !== undefined;
                              const depth = hasParent ? 1 : 0;
                              const parentSelected = hasParent && value.includes(v.parentKey);
                              return (
                                <OptionComponent
                                  key={v.key}
                                  id={v.key}
                                  depth={depth}
                                  isSelected={isSelectedItem(index, this.props)}
                                  isDisabled={parentSelected}
                                  isMultiSelect={isMultiSelect}
                                  {...valueOptions}
                                  {...v}
                                  isMobile={false}
                                  onSelect={this.onSelect}
                                >
                                  {v.value}
                                </OptionComponent>
                              );
                            })
                          }
                        </Scroller> :
                        null
                    }
                  </div>
                );
              }
              }
            </Popper>
          </Portal>
        </Manager>

        {
          isMobile &&
            <Modal
              className={styles.optionsModal}
              size={sizes.EXTRA_SMALL}
              isOpen={isOpen}
              onModalClose={this.onOptionsModalClose}
            >
              <ModalBody
                className={styles.optionsModalBody}
                innerClassName={styles.optionsInnerModalBody}
                scrollDirection={scrollDirections.NONE}
              >
                <Scroller className={styles.optionsModalScroller}>
                  <div className={styles.mobileCloseButtonContainer}>
                    <Link
                      className={styles.mobileCloseButton}
                      onPress={this.onOptionsModalClose}
                    >
                      <Icon
                        name={icons.CLOSE}
                        size={18}
                      />
                    </Link>
                  </div>

                  {
                    values.map((v, index) => {
                      const hasParent = v.parentKey !== undefined;
                      const depth = hasParent ? 1 : 0;
                      const parentSelected = hasParent && value.includes(v.parentKey);
                      return (
                        <OptionComponent
                          key={v.key}
                          id={v.key}
                          depth={depth}
                          isSelected={isSelectedItem(index, this.props)}
                          isMultiSelect={isMultiSelect}
                          isDisabled={parentSelected}
                          {...valueOptions}
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
  value: PropTypes.oneOfType([PropTypes.string, PropTypes.number, PropTypes.arrayOf(PropTypes.number)]).isRequired,
  values: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDisabled: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isEditable: PropTypes.bool.isRequired,
  hasError: PropTypes.bool,
  hasWarning: PropTypes.bool,
  valueOptions: PropTypes.object.isRequired,
  selectedValueOptions: PropTypes.object.isRequired,
  selectedValueComponent: PropTypes.oneOfType([PropTypes.string, PropTypes.func]).isRequired,
  optionComponent: PropTypes.elementType,
  onOpen: PropTypes.func,
  onChange: PropTypes.func.isRequired
};

EnhancedSelectInput.defaultProps = {
  className: styles.enhancedSelect,
  disabledClassName: styles.isDisabled,
  isDisabled: false,
  isFetching: false,
  isEditable: false,
  valueOptions: {},
  selectedValueOptions: {},
  selectedValueComponent: HintedSelectInputSelectedValue,
  optionComponent: HintedSelectInputOption
};

export default EnhancedSelectInput;
