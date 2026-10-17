import { arrayMove, move } from '@dnd-kit/helpers';
import { DragEndEvent, DragOverEvent } from '@dnd-kit/react';
import { useCallback, useMemo, useState } from 'react';
import {
  QualityProfileGroup,
  QualityProfileItems,
  QualityProfileQualityItem,
} from './useQualityProfiles';

export const ROOT_CONTAINER = 'root';

export const qualityKey = (id: number) => `q-${id}`;
export const groupContainerKey = (id: number) => `g-${id}`;

type Board = Record<string, string[]>;

export type DisplayItem =
  | { kind: 'quality'; item: QualityProfileQualityItem }
  | {
      kind: 'group';
      group: QualityProfileGroup;
      items: QualityProfileQualityItem[];
    };

function toBoard(items: QualityProfileItems): Board {
  const board: Board = { [ROOT_CONTAINER]: [] };

  [...items].reverse().forEach((item) => {
    if ('id' in item) {
      board[ROOT_CONTAINER].push(groupContainerKey(item.id));
      board[groupContainerKey(item.id)] = [...item.items]
        .reverse()
        .map((groupItem) => qualityKey(groupItem.quality.id));
    } else {
      board[ROOT_CONTAINER].push(qualityKey(item.quality.id));
    }
  });

  return board;
}

function buildLookups(items: QualityProfileItems) {
  const qualities = new Map<string, QualityProfileQualityItem>();
  const groups = new Map<string, QualityProfileGroup>();

  items.forEach((item) => {
    if ('id' in item) {
      groups.set(groupContainerKey(item.id), item);
      item.items.forEach((groupItem) => {
        qualities.set(qualityKey(groupItem.quality.id), groupItem);
      });
    } else {
      qualities.set(qualityKey(item.quality.id), item);
    }
  });

  return { qualities, groups };
}

function toDisplayItems(
  board: Board,
  items: QualityProfileItems
): DisplayItem[] {
  const { qualities, groups } = buildLookups(items);

  return board[ROOT_CONTAINER].map((key) => {
    const group = groups.get(key);

    if (group) {
      return {
        kind: 'group' as const,
        group,
        items: (board[key] ?? []).map((k) => qualities.get(k)!),
      };
    }

    return { kind: 'quality' as const, item: qualities.get(key)! };
  });
}

function fromBoard(
  board: Board,
  items: QualityProfileItems
): QualityProfileItems {
  const { qualities, groups } = buildLookups(items);

  return board[ROOT_CONTAINER].map<
    QualityProfileGroup | QualityProfileQualityItem
  >((key) => {
    const group = groups.get(key);

    if (group) {
      return {
        ...group,
        items: (board[key] ?? []).map((k) => qualities.get(k)!).reverse(),
      };
    }

    return qualities.get(key)!;
  })
    .filter((item) => !('id' in item) || item.items.length > 0)
    .reverse();
}

function rootIndexOf(board: Board, id: string): number {
  const root = board[ROOT_CONTAINER];
  const direct = root.indexOf(id);

  if (direct !== -1) {
    return direct;
  }

  const containerKey = Object.keys(board).find(
    (key) => key !== ROOT_CONTAINER && board[key].includes(id)
  );

  return containerKey ? root.indexOf(containerKey) : -1;
}

function moveGroup(board: Board, event: DragEndEvent): Board {
  const { source, target } = event.operation;

  if (!source || !target) {
    return board;
  }

  const from = rootIndexOf(board, String(source.id));
  const to = rootIndexOf(board, String(target.id));

  if (from === -1 || to === -1 || from === to) {
    return board;
  }

  return {
    ...board,
    [ROOT_CONTAINER]: arrayMove(board[ROOT_CONTAINER], from, to),
  };
}

export default function useQualityProfileDnd(
  items: QualityProfileItems,
  onItemsChange: (items: QualityProfileItems) => void
) {
  const [board, setBoard] = useState<Board | null>(null);

  const displayItems = useMemo(
    () => toDisplayItems(board ?? toBoard(items), items),
    [board, items]
  );

  const handleDragStart = useCallback(() => {
    setBoard(toBoard(items));
  }, [items]);

  const handleDragOver = useCallback((event: DragOverEvent) => {
    if (event.operation.source?.type === 'group') {
      return;
    }

    setBoard((current) => (current ? move(current, event) : current));
  }, []);

  const handleDragEnd = useCallback(
    (event: DragEndEvent) => {
      setBoard((current) => {
        if (current && !event.canceled) {
          const next =
            event.operation.source?.type === 'group'
              ? moveGroup(current, event)
              : move(current, event);

          onItemsChange(fromBoard(next, items));
        }

        return null;
      });
    },
    [items, onItemsChange]
  );

  return { displayItems, handleDragStart, handleDragOver, handleDragEnd };
}
