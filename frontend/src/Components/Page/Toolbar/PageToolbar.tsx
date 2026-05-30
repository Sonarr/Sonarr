import React, {
  Children,
  createContext,
  isValidElement,
  type PropsWithChildren,
  type ReactElement,
  type ReactNode,
  useCallback,
  useContext,
  useMemo,
  useState,
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

type MoreMenuItem = PageToolbarButtonProps & {
  id: string;
  priority?: number;
  groupId?: string;
  renderOverflow?: (base: PageToolbarButtonProps) => ReactNode;
};

interface ToolbarRegistryContextValue {
  register: (item: MoreMenuItem) => void;
  unregister: (id: string) => void;
}

function shallowEqualMenuItem(a: MoreMenuItem, b: MoreMenuItem): boolean {
  const keys = Object.keys(a) as (keyof MoreMenuItem)[];

  if (keys.length !== Object.keys(b).length) {
    return false;
  }

  return keys.every((key) => key in b && a[key] === b[key]);
}

const ToolbarRegistryContext =
  createContext<ToolbarRegistryContextValue | null>(null);

export function useToolbarRegistry() {
  const ctx = useContext(ToolbarRegistryContext);

  if (!ctx) {
    throw new Error('ToolbarItem must be used inside <PageToolbar>');
  }

  return ctx;
}

interface PageToolbarProps extends PropsWithChildren {
  className?: string;
}

function PageToolbar({
  className = styles.toolbar,
  children,
}: PageToolbarProps) {
  const [items, setItems] = useState<Record<string, MoreMenuItem>>({});

  const register = useCallback((item: MoreMenuItem) => {
    setItems((prev) => {
      const existing = prev[item.id];

      if (existing && shallowEqualMenuItem(existing, item)) {
        return prev;
      }

      return { ...prev, [item.id]: item };
    });
  }, []);

  const unregister = useCallback((id: string) => {
    setItems((prev) => {
      if (!(id in prev)) {
        return prev;
      }

      const { [id]: _removed, ...rest } = prev;
      return rest;
    });
  }, []);

  const registry = useMemo(
    () => ({ register, unregister }),
    [register, unregister]
  );

  const childrenArray = Children.toArray(children).filter(
    (c): c is ReactElement => isValidElement(c)
  );

  const orderById = new Map<string, number>();
  childrenArray.forEach((child, index) => {
    const id = (child.props as { id?: string }).id;

    if (id !== undefined) {
      orderById.set(id, index);
    }
  });

  const moreMenuItems = Object.values(items).sort(
    (a, b) =>
      (orderById.get(a.id) ?? Infinity) - (orderById.get(b.id) ?? Infinity)
  );

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
    <ToolbarRegistryContext.Provider value={registry}>
      <Overflow padding={40}>
        <div className={className}>{rendered}</div>
      </Overflow>
    </ToolbarRegistryContext.Provider>
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

function OverflowMenuItemEntry({ item }: { item: MoreMenuItem }): ReactNode {
  const isVisible = useIsOverflowItemVisible(item.id);

  if (isVisible) {
    return null;
  }

  const {
    id: _id,
    priority: _priority,
    groupId: _groupId,
    renderOverflow,
    ...rest
  } = item;

  return renderOverflow ? (
    renderOverflow(rest)
  ) : (
    <PageToolbarOverflowMenuItem {...rest} />
  );
}

export default PageToolbar;
