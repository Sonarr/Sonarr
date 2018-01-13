import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Measure from 'react-measure';
import classNames from 'classnames';
import { forEach } from 'Helpers/elementChildren';
import { align, icons } from 'Helpers/Props';
import dimensions from 'Styles/Variables/dimensions';
import SpinnerIcon from 'Components/SpinnerIcon';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import MenuItem from 'Components/Menu/MenuItem';
import ToolbarMenuButton from 'Components/Menu/ToolbarMenuButton';
import styles from './PageToolbarSection.css';

const BUTTON_WIDTH = parseInt(dimensions.toolbarButtonWidth);
const SEPARATOR_MARGIN = parseInt(dimensions.toolbarSeparatorMargin);
const SEPARATOR_WIDTH = 2 * SEPARATOR_MARGIN + 1;
const SEPARATOR_NAME = 'PageToolbarSeparator';

function calculateOverflowItems(children, isMeasured, width, collapseButtons) {
  let buttonCount = 0;
  let separatorCount = 0;
  const validChildren = [];

  forEach(children, (child) => {
    const name = child.type.name;

    if (name === SEPARATOR_NAME) {
      separatorCount++;
    } else {
      buttonCount++;
    }

    validChildren.push(child);
  });

  const buttonsWidth = buttonCount * BUTTON_WIDTH;
  const separatorsWidth = separatorCount + SEPARATOR_WIDTH;
  const totalWidth = buttonsWidth + separatorsWidth;

  // If the width of buttons and separators is less than
  // the available width return all valid children.

  if (
    !isMeasured ||
    !collapseButtons ||
    totalWidth < width
  ) {
    return {
      buttons: validChildren,
      buttonCount,
      overflowItems: []
    };
  }

  const maxButtons = Math.max(Math.floor((width - separatorsWidth) / BUTTON_WIDTH), 1);
  const buttons = [];
  const overflowItems = [];
  let actualButtons = 0;

  // Return all buttons if only one is being pushed to the overflow menu.
  if (buttonCount - 1 === maxButtons) {
    return {
      buttons: validChildren,
      buttonCount,
      overflowItems: []
    };
  }

  validChildren.forEach((child, index) => {
    if (actualButtons < maxButtons) {
      if (child.type.name !== SEPARATOR_NAME) {
        buttons.push(child);
        actualButtons++;
      }
    } else if (child.type.name !== SEPARATOR_NAME) {
      overflowItems.push(child.props);
    }
  });

  return {
    buttons,
    buttonCount,
    overflowItems
  };
}

class PageToolbarSection extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isMeasured: false,
      width: 0,
      buttons: [],
      overflowItems: []
    };
  }

  //
  // Listeners

  onMeasure = ({ width }) => {
    this.setState({
      isMeasured: true,
      width
    });
  }

  //
  // Render

  render() {
    const {
      children,
      alignContent,
      collapseButtons
    } = this.props;

    const {
      isMeasured,
      width
    } = this.state;

    const {
      buttons,
      buttonCount,
      overflowItems
    } = calculateOverflowItems(children, isMeasured, width, collapseButtons);

    return (
      <Measure
        whitelist={['width']}
        onMeasure={this.onMeasure}
      >
        <div
          className={styles.sectionContainer}
          style={{
            flexGrow: buttonCount
          }}
        >
          {
            isMeasured ?
              <div className={classNames(
                styles.section,
                styles[alignContent]
              )}
              >
                {
                  buttons.map((button) => {
                    return button;
                  })
                }

                {
                  !!overflowItems.length &&
                    <Menu>
                      <ToolbarMenuButton
                        iconName={icons.OVERFLOW}
                        text="More"
                      />

                      <MenuContent>
                        {
                          overflowItems.map((item) => {
                            const {
                              iconName,
                              spinningName,
                              label,
                              isDisabled,
                              isSpinning,
                              ...otherProps
                            } = item;

                            return (
                              <MenuItem
                                key={label}
                                isDisabled={isDisabled}
                                {...otherProps}
                              >
                                <SpinnerIcon
                                  className={styles.overflowMenuItemIcon}
                                  name={iconName}
                                  spinningName={spinningName}
                                  isDisabled={isDisabled}
                                  isSpinning={isSpinning}
                                />
                                {label}
                              </MenuItem>
                            );
                          })
                        }
                      </MenuContent>
                    </Menu>
                }
              </div> :
              null
          }
        </div>
      </Measure>
    );
  }

}

PageToolbarSection.propTypes = {
  children: PropTypes.node,
  alignContent: PropTypes.oneOf([align.LEFT, align.CENTER, align.RIGHT]),
  collapseButtons: PropTypes.bool.isRequired
};

PageToolbarSection.defaultProps = {
  alignContent: align.LEFT,
  collapseButtons: true
};

export default PageToolbarSection;
