import React, { useMemo } from 'react';
import useMeasure from 'Helpers/Hooks/useMeasure';
import dimensions from 'Styles/Variables/dimensions';
import PageJumpBarItem, { PageJumpBarItemProps } from './PageJumpBarItem';
import styles from './PageJumpBar.css';

const ITEM_HEIGHT = parseInt(dimensions.jumpBarItemHeight);

export interface PageJumpBarItems {
  characters: Record<string, number>;
  order: string[];
}

interface PageJumpBarProps {
  items: PageJumpBarItems;
  minimumItems?: number;
  onItemPress: PageJumpBarItemProps['onItemPress'];
}

function PageJumpBar({
  items,
  minimumItems = 5,
  onItemPress,
}: PageJumpBarProps) {
  const [jumpBarRef, { height }] = useMeasure();

  const visibleItems = useMemo(() => {
    const { characters, order } = items;

    const maximumItems = Math.floor(height / ITEM_HEIGHT);
    const diff = order.length - maximumItems;

    if (diff < 0) {
      return order;
    }

    if (order.length < minimumItems) {
      return order;
    }

    // get first, last, and most common in between to make up numbers
    const result = [order[0]];

    const sorted = order
      .slice(1, -1)
      .map((x) => characters[x])
      .sort((a, b) => b - a);
    const minCount = sorted[maximumItems - 3];
    const greater = sorted.reduce(
      (acc, value) => acc + (value > minCount ? 1 : 0),
      0
    );
    let minAllowed = maximumItems - 2 - greater;

    for (let i = 1; i < order.length - 1; i++) {
      if (characters[order[i]] > minCount) {
        result.push(order[i]);
      } else if (characters[order[i]] === minCount && minAllowed > 0) {
        result.push(order[i]);
        minAllowed--;
      }
    }

    result.push(order[order.length - 1]);

    return result;
  }, [items, height, minimumItems]);

  if (!items.order.length || items.order.length < minimumItems) {
    return null;
  }

  return (
    <div ref={jumpBarRef} className={styles.jumpBar}>
      <div className={styles.jumpBarItems}>
        {visibleItems.map((item) => {
          return (
            <PageJumpBarItem
              key={item}
              label={item}
              onItemPress={onItemPress}
            />
          );
        })}
      </div>
    </div>
  );
}

export default PageJumpBar;
