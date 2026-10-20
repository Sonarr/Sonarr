import React, { useCallback, useLayoutEffect, useRef, useState } from 'react';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import InteractiveSearchCard from './InteractiveSearchCard';
import InteractiveSearchPayload from './InteractiveSearchPayload';
import InteractiveSearchReleaseRow from './InteractiveSearchReleaseRow';
import OverrideMatchModal from './OverrideMatch/OverrideMatchModal';
import ReleaseActions from './ReleaseActions';
import { Release, useGrabRelease } from './useReleases';
import styles from './InteractiveSearchResult.module.css';

interface InteractiveSearchResultProps extends Release {
  index: number;
  style: React.CSSProperties;
  isRowLayout: boolean;
  searchPayload: InteractiveSearchPayload;
  setRowHeight: (index: number, height: number) => void;
}

function InteractiveSearchResult({
  index,
  style,
  isRowLayout,
  searchPayload,
  setRowHeight,
  ...item
}: InteractiveSearchResultProps) {
  const {
    release,
    parsedInfo,
    languages,
    mappedSeriesId,
    mappedEpisodeInfo,
    downloadAllowed,
  } = item;

  const { guid, indexerId, title, protocol } = release;

  const containerRef = useRef<HTMLDivElement>(null);
  const [isConfirmGrabModalOpen, setIsConfirmGrabModalOpen] = useState(false);
  const [isOverrideModalOpen, setIsOverrideModalOpen] = useState(false);
  const { isGrabbing, isGrabbed, grabError, grabRelease } = useGrabRelease();

  useLayoutEffect(() => {
    const element = containerRef.current;

    if (!element) {
      return;
    }

    const measure = () => setRowHeight(index, element.offsetHeight);

    measure();

    const observer = new ResizeObserver(measure);
    observer.observe(element);

    return () => observer.disconnect();
  }, [index, setRowHeight]);

  const handleGrabPress = useCallback(() => {
    if (downloadAllowed) {
      grabRelease({
        guid,
        indexerId,
      });

      return;
    }

    setIsConfirmGrabModalOpen(true);
  }, [guid, indexerId, downloadAllowed, grabRelease]);

  const handleGrabConfirm = useCallback(() => {
    setIsConfirmGrabModalOpen(false);

    grabRelease({
      guid,
      indexerId,
      searchInfo: searchPayload,
    });
  }, [guid, indexerId, searchPayload, grabRelease]);

  const handleGrabCancel = useCallback(() => {
    setIsConfirmGrabModalOpen(false);
  }, []);

  const handleOverridePress = useCallback(() => {
    setIsOverrideModalOpen(true);
  }, []);

  const handleOverrideModalClose = useCallback(() => {
    setIsOverrideModalOpen(false);
  }, []);

  const { height: _height, ...positionStyle } = style;

  const Layout = isRowLayout
    ? InteractiveSearchReleaseRow
    : InteractiveSearchCard;

  return (
    <div ref={containerRef} className={styles.container} style={positionStyle}>
      <Layout
        {...item}
        actions={
          <ReleaseActions
            isGrabbing={isGrabbing}
            isGrabbed={isGrabbed}
            grabError={grabError}
            onGrabPress={handleGrabPress}
            onOverridePress={handleOverridePress}
          />
        }
      />

      <ConfirmModal
        isOpen={isConfirmGrabModalOpen}
        kind={kinds.WARNING}
        title={translate('GrabRelease')}
        message={translate('GrabReleaseUnknownSeriesOrEpisodeMessageText', {
          title,
        })}
        confirmLabel={translate('Grab')}
        onConfirm={handleGrabConfirm}
        onCancel={handleGrabCancel}
      />

      <OverrideMatchModal
        isOpen={isOverrideModalOpen}
        title={title}
        indexerId={indexerId}
        guid={guid}
        seriesId={mappedSeriesId}
        episodes={mappedEpisodeInfo}
        languages={languages}
        quality={parsedInfo.quality}
        protocol={protocol}
        isGrabbing={isGrabbing}
        grabError={grabError}
        grabRelease={grabRelease}
        onModalClose={handleOverrideModalClose}
      />
    </div>
  );
}

export default InteractiveSearchResult;
