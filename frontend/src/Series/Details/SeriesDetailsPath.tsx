import copy from 'copy-to-clipboard';
import React, {
  useCallback,
  useEffect,
  useLayoutEffect,
  useMemo,
  useState,
} from 'react';
import Icon from 'Components/Icon';
import ClipboardButton from 'Components/Link/ClipboardButton';
import Link from 'Components/Link/Link';
import StatusIndicator from 'Components/StatusIndicator';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, sizes, tooltipPositions } from 'Helpers/Props';
import SeriesMovePending from 'Series/SeriesMovePending';
import { isMobile as isMobileUtil } from 'Utilities/browser';
import translate from 'Utilities/String/translate';
import styles from './SeriesDetailsPath.module.css';

type CopyState = 'success' | 'error' | null;

interface SeriesDetailsPathProps {
  path: string;
  pendingPath?: string;
}

function SeriesDetailsPath({ path, pendingPath }: SeriesDetailsPathProps) {
  const [state, setState] = useState<CopyState>(null);
  const isMobile = useMemo(() => isMobileUtil(), []);
  const [textElement, setTextElement] = useState<HTMLSpanElement | null>(null);
  const [isTruncated, setIsTruncated] = useState(false);

  useLayoutEffect(() => {
    if (!textElement) {
      return;
    }

    const compute = () =>
      setIsTruncated(textElement.scrollWidth > textElement.clientWidth);

    compute();

    const observer = new ResizeObserver(compute);
    observer.observe(textElement);

    return () => observer.disconnect();
  }, [textElement, path]);

  useEffect(() => {
    if (!state) {
      return;
    }

    const timeoutId = setTimeout(() => {
      setState(null);
    }, 3000);

    return () => {
      clearTimeout(timeoutId);
    };
  }, [state]);

  const handlePress = useCallback(async () => {
    try {
      if ('clipboard' in navigator) {
        await navigator.clipboard.writeText(path);
      } else {
        copy(path);
      }

      setState('success');
    } catch (e) {
      setState('error');
      console.error('Failed to copy to clipboard', e);
    }
  }, [path]);

  let pathElement: React.ReactNode = null;

  if (isMobile) {
    pathElement = (
      <Tooltip
        className={styles.pathContainer}
        anchor={
          <span className={styles.path}>
            <span className={styles.icon}>
              <Icon name={icons.FOLDER} size={14} aria-hidden={true} />
            </span>

            <span className={styles.text}>{path}</span>
          </span>
        }
        tooltip={
          <div className={styles.tooltip}>
            <span className={styles.tooltipPath}>{path}</span>

            <ClipboardButton
              className={styles.copyButton}
              value={path}
              kind={kinds.DEFAULT}
              size={sizes.SMALL}
            />
          </div>
        }
        contentRole="dialog"
        kind={kinds.DEFAULT}
        position={tooltipPositions.BOTTOM}
      />
    );
  } else {
    const anchor = (
      <Link className={styles.path} onPress={handlePress}>
        {state ? (
          <StatusIndicator
            className={styles.icon}
            label={translate(
              state === 'error' ? 'CopyToClipboardError' : 'CopiedToClipboard'
            )}
            role={state === 'error' ? 'alert' : 'status'}
            aria-atomic={true}
          >
            <Icon
              name={state === 'error' ? icons.DANGER : icons.CHECK}
              kind={state === 'error' ? kinds.DANGER : kinds.SUCCESS}
              size={14}
              aria-hidden={true}
            />
          </StatusIndicator>
        ) : (
          <span className={styles.icon}>
            <Icon
              className={styles.folderIcon}
              name={icons.FOLDER}
              size={14}
              aria-hidden={true}
            />
            <Icon
              className={styles.copyIcon}
              name={icons.CLIPBOARD}
              size={14}
              aria-hidden={true}
            />
          </span>
        )}

        <span ref={setTextElement} className={styles.text}>
          {path}
        </span>
      </Link>
    );

    pathElement = isTruncated ? (
      <Tooltip
        className={styles.pathContainer}
        anchor={anchor}
        tooltip={<span>{path}</span>}
        kind={kinds.DEFAULT}
        position={tooltipPositions.BOTTOM}
        isAnchorFocusable={false}
      />
    ) : (
      <span className={styles.pathContainer}>{anchor}</span>
    );
  }

  return (
    <span className={styles.container}>
      {pathElement}

      {pendingPath ? (
        <SeriesMovePending
          className={styles.pendingIcon}
          pendingPath={pendingPath}
        />
      ) : null}
    </span>
  );
}

export default SeriesDetailsPath;
