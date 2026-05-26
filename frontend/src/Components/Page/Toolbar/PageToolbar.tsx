import classNames from 'classnames';
import React, {
  Children,
  isValidElement,
  type ReactElement,
  type ReactNode,
} from 'react';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import ToolbarMenuButton from 'Components/Menu/ToolbarMenuButton';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import {
  Overflow,
  useIsOverflowItemVisible,
  useOverflowMenu,
} from './Overflow';
import { PageToolbarButtonProps } from './PageToolbarButton';
import PageToolbarOverflowMenuItem from './PageToolbarOverflowMenuItem';
import PageToolbarSpacer from './PageToolbarSpacer';
import styles from './PageToolbar.css';

export type MoreMenuItem = PageToolbarButtonProps & {
  id: string;
  renderOverflow?: (base: PageToolbarButtonProps) => ReactNode;
};

interface PageToolbarProps {
  className?: string;
  children?: ReactNode;
  moreMenuItems?: MoreMenuItem[];
}

function PageToolbar({
  className = styles.toolbar,
  children,
  moreMenuItems = [],
}: PageToolbarProps) {
  const childrenArray = Children.toArray(children).filter(
    isValidElement
  ) as ReactElement[];
  const spacerIndex = childrenArray.findIndex(
    (c) => c.type === PageToolbarSpacer
  );

  const moreInsertBefore =
    spacerIndex >= 0 ? spacerIndex : childrenArray.length;

  const rendered = childrenArray.flatMap((child, i) => {
    if (i === moreInsertBefore) {
      return [<MoreButton key="more" items={moreMenuItems} />, child];
    }
    return [child];
  });
  if (spacerIndex < 0) {
    rendered.push(<MoreButton key="more-end" items={moreMenuItems} />);
  }

  return (
    <Overflow padding={40}>
      <div className={classNames(className)}>{rendered}</div>
    </Overflow>
  );
}

interface MoreButtonProps {
  items: MoreMenuItem[];
}

// Skips render when nothing overflows so ⋮ stays out of the flex flow.
function MoreButton({ items }: MoreButtonProps) {
  const { ref, isOverflowing } = useOverflowMenu<HTMLSpanElement>();

  if (!isOverflowing) {
    return null;
  }

  return (
    <span ref={ref} className={styles.moreContainer}>
      <Menu>
        <ToolbarMenuButton
          className={styles.overflowMenuButton}
          iconName={icons.OVERFLOW}
          aria-label={translate('More')}
        />
        <MenuContent>
          {items.map((item) => (
            <OverflowMenuItemEntry key={item.id} item={item} />
          ))}
        </MenuContent>
      </Menu>
    </span>
  );
}

function OverflowMenuItemEntry({ item }: { item: MoreMenuItem }) {
  const isVisible = useIsOverflowItemVisible(item.id);

  if (isVisible) {
    return null;
  }

  const { id: _id, renderOverflow, ...rest } = item;

  return renderOverflow ? (
    <>{renderOverflow(rest)}</>
  ) : (
    <PageToolbarOverflowMenuItem {...rest} />
  );
}

export default PageToolbar;
