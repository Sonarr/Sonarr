import classNames from 'classnames';
import React, { useLayoutEffect, useMemo, useState } from 'react';
import Label from 'Components/Label';
import SeriesTagList from 'Components/SeriesTagList';
import Tooltip from 'Components/Tooltip/Tooltip';
import { kinds, tooltipPositions } from 'Helpers/Props';
import { useSortedTagList } from 'Tags/useTags';
import styles from './SeriesIndexTags.module.css';

interface SeriesIndexTagsProps {
  tags: number[];
}

interface Layout {
  hiddenTagIds: number[];
  hiddenCountLeft: number;
}

function getOuterWidth(element: HTMLElement) {
  const { marginLeft, marginRight } = getComputedStyle(element);

  return element.offsetWidth + parseFloat(marginLeft) + parseFloat(marginRight);
}

const emptyLayout: Layout = { hiddenTagIds: [], hiddenCountLeft: 0 };

function isSameLayout(a: Layout, b: Layout) {
  return (
    a.hiddenCountLeft === b.hiddenCountLeft &&
    a.hiddenTagIds.length === b.hiddenTagIds.length &&
    a.hiddenTagIds.every((id, index) => id === b.hiddenTagIds[index])
  );
}

function SeriesIndexTags({ tags }: SeriesIndexTagsProps) {
  const sortedTagList = useSortedTagList();
  const [tagsElement, setTagsElement] = useState<HTMLDivElement | null>(null);
  const [layout, setLayout] = useState<Layout>(emptyLayout);

  const seriesTags = useMemo(() => {
    return sortedTagList.filter((tag) => tags.includes(tag.id));
  }, [tags, sortedTagList]);

  const { hiddenTagIds, hiddenCountLeft } = layout;
  const hiddenCount = hiddenTagIds.length;

  useLayoutEffect(() => {
    if (!tagsElement) {
      return;
    }

    const updateLayout = (nextLayout: Layout) => {
      setLayout((prevLayout) =>
        isSameLayout(prevLayout, nextLayout) ? prevLayout : nextLayout
      );
    };

    const compute = () => {
      const labels = Array.from(
        tagsElement.querySelectorAll<HTMLElement>('[data-tag-id]')
      ).map((element) => ({
        id: Number(element.dataset.tagId),
        width: getOuterWidth(element),
      }));

      const availableWidth = tagsElement.clientWidth;
      const totalWidth = labels.reduce((acc, { width }) => acc + width, 0);

      if (totalWidth <= availableWidth) {
        updateLayout(emptyLayout);
        return;
      }

      const hiddenCountElement = tagsElement.querySelector<HTMLElement>(
        `.${styles.hiddenCount}`
      );

      const hiddenCountWidth = hiddenCountElement
        ? getOuterWidth(hiddenCountElement)
        : 0;

      const nextHiddenTagIds: number[] = [];
      let left = 0;

      labels.forEach(({ id, width }) => {
        if (
          !nextHiddenTagIds.length &&
          left + width + hiddenCountWidth <= availableWidth
        ) {
          left += width;
        } else {
          nextHiddenTagIds.push(id);
        }
      });

      updateLayout({ hiddenTagIds: nextHiddenTagIds, hiddenCountLeft: left });
    };

    compute();

    const observer = new ResizeObserver(compute);
    observer.observe(tagsElement);

    return () => observer.disconnect();
  }, [tagsElement, seriesTags, hiddenCount]);

  return (
    <Tooltip
      className={styles.tooltipAnchor}
      anchor={
        <div
          ref={setTagsElement}
          className={styles.tags}
          style={
            {
              '--hiddenCountLeft': `${hiddenCountLeft}px`,
            } as React.CSSProperties
          }
        >
          {seriesTags.map((tag) => (
            <Label
              key={tag.id}
              className={
                hiddenTagIds.includes(tag.id) ? styles.hiddenTag : undefined
              }
              kind={kinds.INFO}
              data-tag-id={tag.id}
            >
              {tag.label}
            </Label>
          ))}

          <span
            className={classNames(
              styles.hiddenCount,
              hiddenCount === 0 && styles.isHidden
            )}
          >
            +{Math.max(hiddenCount, 1)}
          </span>
        </div>
      }
      tooltip={<SeriesTagList tags={tags} />}
      kind={kinds.DEFAULT}
      position={tooltipPositions.BOTTOM}
      isAnchorFocusable={false}
      isDisabled={hiddenCount === 0}
    />
  );
}

export default SeriesIndexTags;
