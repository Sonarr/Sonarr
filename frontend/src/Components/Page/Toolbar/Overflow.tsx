import {
  createOverflowManager,
  type OverflowManager,
} from '@fluentui/priority-overflow';
import React, {
  cloneElement,
  createContext,
  type ReactElement,
  type ReactNode,
  type RefAttributes,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import slotStyles from './ToolbarItem.css';

interface OverflowCtx {
  manager: OverflowManager;
  visibility: Record<string, boolean>;
  overflowCount: number;
}

const OverflowContext = createContext<OverflowCtx | null>(null);

function useOverflowCtx() {
  const ctx = useContext(OverflowContext);

  if (!ctx) {
    throw new Error('Overflow primitives must be inside <Overflow>');
  }

  return ctx;
}

interface OverflowProps {
  children: ReactElement;
  padding?: number;
}

export function Overflow({ children, padding = 10 }: OverflowProps) {
  const manager = useMemo(() => createOverflowManager(), []);
  const [visibility, setVisibility] = useState<Record<string, boolean>>({});
  const [overflowCount, setOverflowCount] = useState(0);

  // `padding` must match the container's horizontal padding and stay stable per mount; a change re-observes and drops registered items.
  const initialPadding = useRef(padding);
  useEffect(() => {
    if (!window.Sonarr.isProduction && padding !== initialPadding.current) {
      // eslint-disable-next-line no-console
      console.error(
        `Overflow: \`padding\` changed from ${initialPadding.current} to ${padding} after mount; registered items will be dropped. Keep it stable.`
      );
    }
  }, [padding]);

  const setContainer = useCallback(
    (node: HTMLElement | null) => {
      if (node) {
        manager.observe(node, {
          padding,
          onUpdateItemVisibility: ({ item, visible }) => {
            // Fluent auto-tags dividers but not items — mirror it here so one CSS rule hides both.
            item.element.toggleAttribute('data-overflowing', !visible);
            setVisibility((prev) =>
              prev[item.id] === visible ? prev : { ...prev, [item.id]: visible }
            );
          },
          onUpdateOverflow: ({ invisibleItems }) => {
            setOverflowCount(invisibleItems.length);
          },
        });
      } else {
        manager.disconnect();
      }
    },
    [manager, padding]
  );

  return (
    <OverflowContext.Provider value={{ manager, visibility, overflowCount }}>
      {cloneElement(children, {
        ref: setContainer,
      } as RefAttributes<HTMLElement>)}
    </OverflowContext.Provider>
  );
}

interface InternalOverflowItemProps {
  id: string;
  priority?: number;
  pinned?: boolean;
  groupId?: string;
  children: ReactElement;
}

export function InternalOverflowItem({
  id,
  priority,
  pinned,
  groupId,
  children,
}: InternalOverflowItemProps) {
  const { manager } = useOverflowCtx();

  const setNode = useCallback(
    (node: HTMLElement | null) => {
      if (node) {
        manager.addItem({
          element: node,
          id,
          priority: priority ?? 0,
          pinned,
          groupId,
        });
      } else {
        manager.removeItem(id);
      }
    },
    [manager, id, priority, pinned, groupId]
  );

  return cloneElement(children, { ref: setNode } as RefAttributes<HTMLElement>);
}

interface OverflowDividerProps {
  groupId: string;
  children: ReactNode;
}

// Slot div wraps the separator so its gap is inside Fluent's border-box budget (margins aren't measured).
export function OverflowDivider({ groupId, children }: OverflowDividerProps) {
  const { manager } = useOverflowCtx();

  const setNode = useCallback(
    (node: HTMLElement | null) => {
      if (node) {
        manager.addDivider({ element: node, groupId });
      } else {
        manager.removeDivider(groupId);
      }
    },
    [manager, groupId]
  );

  return (
    <div ref={setNode} className={slotStyles.slot}>
      {children}
    </div>
  );
}

export function useOverflowMenu<T extends HTMLElement>() {
  const { manager, overflowCount } = useOverflowCtx();
  const ref = useCallback(
    (node: T | null) => {
      if (node) {
        manager.addOverflowMenu(node);
      } else {
        manager.removeOverflowMenu();
      }
    },
    [manager]
  );
  return {
    ref,
    isOverflowing: overflowCount > 0,
    overflowCount,
  };
}

export function useIsOverflowItemVisible(id: string) {
  const { visibility } = useOverflowCtx();
  return visibility[id] ?? true;
}
