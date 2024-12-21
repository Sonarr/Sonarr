import classNames from 'classnames';
import React, { ReactElement, useMemo } from 'react';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import ToolbarMenuButton from 'Components/Menu/ToolbarMenuButton';
import useMeasure from 'Helpers/Hooks/useMeasure';
import { icons } from 'Helpers/Props';
import { Align } from 'Helpers/Props/align';
import dimensions from 'Styles/Variables/dimensions';
import translate from 'Utilities/String/translate';
import { PageToolbarButtonProps } from './PageToolbarButton';
import PageToolbarOverflowMenuItem from './PageToolbarOverflowMenuItem';
import styles from './PageToolbarSection.css';

const BUTTON_WIDTH = parseInt(dimensions.toolbarButtonWidth);
const SEPARATOR_MARGIN = parseInt(dimensions.toolbarSeparatorMargin);
const SEPARATOR_WIDTH = 2 * SEPARATOR_MARGIN + 1;

interface PageToolbarSectionProps {
  children?:
    | (ReactElement<PageToolbarButtonProps> | ReactElement<never>)
    | (ReactElement<PageToolbarButtonProps> | ReactElement<never>)[];
  alignContent?: Extract<Align, keyof typeof styles>;
  collapseButtons?: boolean;
}

function PageToolbarSection({
  children,
  alignContent = 'left',
  collapseButtons = true,
}: PageToolbarSectionProps) {
  const [sectionRef, { width }] = useMeasure();
  const isMeasured = width > 0;

  const { buttons, buttonCount, overflowItems } = useMemo(() => {
    let buttonCount = 0;
    let separatorCount = 0;
    const validChildren: ReactElement[] = [];

    React.Children.forEach(children, (child) => {
      if (!child) {
        return;
      }

      if (Object.keys(child.props).length === 0) {
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

    if (!isMeasured || !collapseButtons || totalWidth < width) {
      return {
        buttons: validChildren,
        buttonCount,
        overflowItems: [],
      };
    }

    const maxButtons = Math.max(
      Math.floor((width - separatorsWidth) / BUTTON_WIDTH),
      1
    );

    const buttons: ReactElement<PageToolbarButtonProps>[] = [];
    const overflowItems: PageToolbarButtonProps[] = [];

    let actualButtons = 0;

    // Return all buttons if only one is being pushed to the overflow menu.
    if (buttonCount - 1 === maxButtons) {
      const overflowItems: PageToolbarButtonProps[] = [];

      return {
        buttons: validChildren,
        buttonCount,
        overflowItems,
      };
    }

    validChildren.forEach((child) => {
      const isSeparator = Object.keys(child.props).length === 0;

      if (actualButtons < maxButtons) {
        if (!isSeparator) {
          buttons.push(child);
          actualButtons++;
        }
      } else if (!isSeparator) {
        overflowItems.push(child.props);
      }
    });

    return {
      buttons,
      buttonCount,
      overflowItems,
    };
  }, [children, isMeasured, width, collapseButtons]);

  return (
    <div
      ref={sectionRef}
      className={styles.sectionContainer}
      style={{
        flexGrow: buttonCount,
      }}
    >
      {isMeasured ? (
        <div className={classNames(styles.section, styles[alignContent])}>
          {buttons.map((button) => {
            return button;
          })}

          {overflowItems.length ? (
            <Menu>
              <ToolbarMenuButton
                className={styles.overflowMenuButton}
                iconName={icons.OVERFLOW}
                text={translate('More')}
              />

              <MenuContent>
                {overflowItems.map((item) => {
                  const {
                    label,
                    overflowComponent:
                      OverflowComponent = PageToolbarOverflowMenuItem,
                  } = item;

                  return <OverflowComponent key={label} {...item} />;
                })}
              </MenuContent>
            </Menu>
          ) : null}
        </div>
      ) : null}
    </div>
  );
}

export default PageToolbarSection;
