import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import PageSectionContent from 'Components/Page/PageSectionContent';
import Scroller from 'Components/Scroller/Scroller';
import { icons, scrollDirections } from 'Helpers/Props';
import {
  fetchDelayProfiles,
  reorderDelayProfile,
} from 'Store/Actions/settingsActions';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import DelayProfileModel from 'typings/DelayProfile';
import translate from 'Utilities/String/translate';
import DelayProfile from './DelayProfile';
import EditDelayProfileModal from './EditDelayProfileModal';
import styles from './DelayProfiles.css';

function createDisplayProfilesSelector() {
  return createSelector(
    (state: AppState) => state.settings.delayProfiles,
    (delayProfiles) => {
      const { defaultProfile, items } = delayProfiles.items.reduce<{
        defaultProfile: null | DelayProfileModel;
        items: DelayProfileModel[];
      }>(
        (acc, item) => {
          if (item.id === 1) {
            acc.defaultProfile = item;
          } else {
            acc.items.push(item);
          }

          return acc;
        },
        {
          defaultProfile: null,
          items: [],
        }
      );

      items.sort((a, b) => a.order - b.order);

      return {
        defaultProfile,
        ...delayProfiles,
        items,
      };
    }
  );
}

function DelayProfiles() {
  const dispatch = useDispatch();

  const { error, isFetching, isPopulated, items, defaultProfile } = useSelector(
    createDisplayProfilesSelector()
  );

  const tagList = useSelector(createTagsSelector());

  const [dragIndex, setDragIndex] = useState<number | null>(null);
  const [dropIndex, setDropIndex] = useState<number | null>(null);
  const [isAddDelayProfileModalOpen, setIsAddDelayProfileModalOpen] =
    useState(false);

  const isDragging = dropIndex !== null;
  const isDraggingUp =
    isDragging &&
    dropIndex != null &&
    dragIndex != null &&
    dropIndex < dragIndex;
  const isDraggingDown =
    isDragging &&
    dropIndex != null &&
    dragIndex != null &&
    dropIndex > dragIndex;

  const handleAddDelayProfilePress = useCallback(() => {
    setIsAddDelayProfileModalOpen(true);
  }, []);

  const handleAddDelayProfileModalClose = useCallback(() => {
    setIsAddDelayProfileModalOpen(false);
  }, []);

  const handleDelayProfileDragMove = useCallback(
    (newDragIndex: number, newDropIndex: number) => {
      setDragIndex(newDragIndex);
      setDropIndex(newDropIndex);
    },
    []
  );

  const handleDelayProfileDragEnd = useCallback(
    (id: number, didDrop: boolean) => {
      if (didDrop && dropIndex !== null) {
        dispatch(reorderDelayProfile({ id, moveIndex: dropIndex - 1 }));
      }

      setDragIndex(null);
      setDropIndex(null);
    },
    [dropIndex, dispatch]
  );

  useEffect(() => {
    dispatch(fetchDelayProfiles());
  }, [dispatch]);

  return (
    <FieldSet legend={translate('DelayProfiles')}>
      <PageSectionContent
        errorMessage={translate('DelayProfilesLoadError')}
        error={error}
        isFetching={isFetching}
        isPopulated={isPopulated}
      >
        <Scroller
          className={styles.horizontalScroll}
          scrollDirection={scrollDirections.HORIZONTAL}
          autoFocus={false}
        >
          <div>
            <div className={styles.delayProfilesHeader}>
              <div className={styles.column}>
                {translate('PreferredProtocol')}
              </div>
              <div className={styles.column}>{translate('UsenetDelay')}</div>
              <div className={styles.column}>{translate('TorrentDelay')}</div>
              <div className={styles.tags}>{translate('Tags')}</div>
            </div>

            <div className={styles.delayProfiles}>
              {items.map((item) => {
                return (
                  <DelayProfile
                    key={item.id}
                    {...item}
                    tagList={tagList}
                    isDraggingUp={isDraggingUp}
                    isDraggingDown={isDraggingDown}
                    onDelayProfileDragEnd={handleDelayProfileDragEnd}
                    onDelayProfileDragMove={handleDelayProfileDragMove}
                  />
                );
              })}
            </div>

            {defaultProfile ? (
              <div>
                <DelayProfile
                  {...defaultProfile}
                  tagList={tagList}
                  isDraggingDown={false}
                  isDraggingUp={false}
                  onDelayProfileDragEnd={handleDelayProfileDragEnd}
                  onDelayProfileDragMove={handleDelayProfileDragMove}
                />
              </div>
            ) : null}
          </div>
        </Scroller>

        <div className={styles.addDelayProfile}>
          <Link
            className={styles.addButton}
            onPress={handleAddDelayProfilePress}
          >
            <Icon name={icons.ADD} />
          </Link>
        </div>

        <EditDelayProfileModal
          isOpen={isAddDelayProfileModalOpen}
          onModalClose={handleAddDelayProfileModalClose}
        />
      </PageSectionContent>
    </FieldSet>
  );
}

export default DelayProfiles;
