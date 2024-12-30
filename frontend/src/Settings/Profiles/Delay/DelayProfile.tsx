import classNames from 'classnames';
import React, { useCallback, useMemo, useRef, useState } from 'react';
import { DragSourceMonitor, useDrag, useDrop, XYCoord } from 'react-dnd';
import { useDispatch } from 'react-redux';
import { Tag } from 'App/State/TagsAppState';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TagList from 'Components/TagList';
import DragType from 'Helpers/DragType';
import { icons, kinds } from 'Helpers/Props';
import { deleteDelayProfile } from 'Store/Actions/settingsActions';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';
import EditDelayProfileModal from './EditDelayProfileModal';
import styles from './DelayProfile.css';

function getDelay(enabled: boolean, delay: number) {
  if (!enabled) {
    return '-';
  }

  if (!delay) {
    return translate('NoDelay');
  }

  if (delay === 1) {
    return translate('OneMinute');
  }

  // TODO: use better units of time than just minutes
  return translate('DelayMinutes', { delay });
}

interface DragItem {
  id: number;
  order: number;
}

interface DelayProfileProps {
  id: number;
  enableUsenet: boolean;
  enableTorrent: boolean;
  preferredProtocol: string;
  usenetDelay: number;
  torrentDelay: number;
  order: number;
  tags: number[];
  tagList: Tag[];
  isDraggingDown: boolean;
  isDraggingUp: boolean;
  onDelayProfileDragEnd: (id: number, didDrop: boolean) => void;
  onDelayProfileDragMove: (dragIndex: number, hoverIndex: number) => void;
}

function DelayProfile({
  id,
  enableUsenet,
  enableTorrent,
  preferredProtocol,
  usenetDelay,
  torrentDelay,
  order,
  tags,
  tagList,
  isDraggingDown,
  isDraggingUp,
  onDelayProfileDragEnd,
  onDelayProfileDragMove,
}: DelayProfileProps) {
  const dispatch = useDispatch();
  const ref = useRef<HTMLDivElement>(null);

  const [isEditDelayProfileModalOpen, setIsEditDelayProfileModalOpen] =
    useState(false);

  const [isDeleteDelayProfileModalOpen, setIsDeleteDelayProfileModalOpen] =
    useState(false);

  const preferred = useMemo(() => {
    if (!enableUsenet) {
      return translate('OnlyTorrent');
    } else if (!enableTorrent) {
      return translate('OnlyUsenet');
    }

    return titleCase(translate('PreferProtocol', { preferredProtocol }));
  }, [preferredProtocol, enableUsenet, enableTorrent]);

  const handleEditDelayProfilePress = useCallback(() => {
    setIsEditDelayProfileModalOpen(true);
  }, []);

  const handleEditDelayProfileModalClose = useCallback(() => {
    setIsEditDelayProfileModalOpen(false);
  }, []);

  const handleDeleteDelayProfilePress = useCallback(() => {
    setIsEditDelayProfileModalOpen(false);
    setIsDeleteDelayProfileModalOpen(true);
  }, []);

  const handleDeleteDelayProfileModalClose = useCallback(() => {
    setIsDeleteDelayProfileModalOpen(false);
  }, []);

  const handleConfirmDeleteDelayProfile = useCallback(() => {
    dispatch(deleteDelayProfile(id));
  }, [id, dispatch]);

  const [{ isOver }, dropRef] = useDrop<DragItem, void, { isOver: boolean }>({
    accept: DragType.DelayProfile,
    collect(monitor) {
      return {
        isOver: monitor.isOver(),
      };
    },
    hover(item: DragItem, monitor) {
      if (!ref.current) {
        return;
      }
      const dragIndex = item.order;
      const hoverIndex = order;

      // Don't replace items with themselves
      if (dragIndex === hoverIndex) {
        return;
      }

      const hoverBoundingRect = ref.current?.getBoundingClientRect();
      const hoverMiddleY =
        (hoverBoundingRect.bottom - hoverBoundingRect.top) / 2;
      const clientOffset = monitor.getClientOffset();
      const hoverClientY = (clientOffset as XYCoord).y - hoverBoundingRect.top;

      // When moving up, only trigger if drag position is above 50% and
      // when moving down, only trigger if drag position is below 50%.
      // If we're moving down the hoverIndex needs to be increased
      // by one so it's ordered properly. Otherwise the hoverIndex will work.

      if (dragIndex < hoverIndex && hoverClientY > hoverMiddleY) {
        onDelayProfileDragMove(dragIndex, hoverIndex + 1);
      } else if (dragIndex > hoverIndex && hoverClientY < hoverMiddleY) {
        onDelayProfileDragMove(dragIndex, hoverIndex);
      }
    },
  });

  const [{ isDragging }, dragRef, previewRef] = useDrag<
    DragItem,
    unknown,
    { isDragging: boolean }
  >({
    type: DragType.DelayProfile,
    item: () => {
      return {
        id,
        order,
      };
    },
    collect: (monitor: DragSourceMonitor<unknown, unknown>) => ({
      isDragging: monitor.isDragging(),
    }),
    end: (item: DragItem, monitor) => {
      onDelayProfileDragEnd(item.id, monitor.didDrop());
    },
  });

  dropRef(previewRef(ref));

  const isBefore = !isDragging && isDraggingUp && isOver;
  const isAfter = !isDragging && isDraggingDown && isOver;

  return (
    <div ref={id === 1 ? undefined : ref}>
      {isBefore ? (
        <div
          className={classNames(styles.placeholder, styles.placeholderBefore)}
        />
      ) : null}

      <div
        className={classNames(
          styles.delayProfile,
          isDragging && styles.isDragging
        )}
      >
        <div className={styles.column}>{preferred}</div>
        <div className={styles.column}>
          {getDelay(enableUsenet, usenetDelay)}
        </div>
        <div className={styles.column}>
          {getDelay(enableTorrent, torrentDelay)}
        </div>

        <TagList tags={tags} tagList={tagList} />

        <div className={styles.actions}>
          <Link
            className={id === 1 ? styles.editButton : undefined}
            onPress={handleEditDelayProfilePress}
          >
            <Icon name={icons.EDIT} />
          </Link>

          {id === 1 ? null : (
            <div ref={dragRef} className={styles.dragHandle}>
              <Icon className={styles.dragIcon} name={icons.REORDER} />
            </div>
          )}
        </div>
      </div>

      {isAfter ? (
        <div
          className={classNames(styles.placeholder, styles.placeholderAfter)}
        />
      ) : null}

      <EditDelayProfileModal
        id={id}
        isOpen={isEditDelayProfileModalOpen}
        onModalClose={handleEditDelayProfileModalClose}
        onDeleteDelayProfilePress={handleDeleteDelayProfilePress}
      />

      <ConfirmModal
        isOpen={isDeleteDelayProfileModalOpen}
        kind={kinds.DANGER}
        title={translate('DeleteDelayProfile')}
        message={translate('DeleteDelayProfileMessageText')}
        confirmLabel={translate('Delete')}
        onConfirm={handleConfirmDeleteDelayProfile}
        onCancel={handleDeleteDelayProfileModalClose}
      />
    </div>
  );
}

export default DelayProfile;
