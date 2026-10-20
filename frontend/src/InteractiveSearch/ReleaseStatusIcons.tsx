import React, { useMemo } from 'react';
import Icon from 'Components/Icon';
import Popover from 'Components/Tooltip/Popover';
import IndexerFlags from 'Episode/IndexerFlags';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import { useUiSettingsValues } from 'Settings/UI/useUiSettings';
import formatDateTime from 'Utilities/Date/formatDateTime';
import translate from 'Utilities/String/translate';
import { Release } from './useReleases';
import styles from './ReleaseStatusIcons.module.css';

type ReleaseStatusIconsProps = Pick<
  Release,
  'decision' | 'history' | 'release'
>;

function ReleaseStatusIcons({
  decision,
  history,
  release,
}: ReleaseStatusIconsProps) {
  const { rejections = [] } = decision;
  const { indexerFlags = 0 } = release;
  const { longDateFormat, timeFormat } = useUiSettingsValues();

  const isBlocklisted = useMemo(() => {
    return (
      decision.rejections.findIndex((r) => r.reason === 'blocklisted') >= 0
    );
  }, [decision]);

  return (
    <>
      {indexerFlags ? (
        <Popover
          className={styles.popover}
          anchor={<Icon name={icons.FLAG} />}
          title={translate('IndexerFlags')}
          body={<IndexerFlags indexerFlags={indexerFlags} />}
          position={tooltipPositions.BOTTOM}
        />
      ) : null}

      {history ? (
        <Icon
          name={icons.DOWNLOADING}
          kind={history.failed ? kinds.DANGER : kinds.DEFAULT}
          title={`${
            history.failed
              ? translate('FailedAt', {
                  date: formatDateTime(
                    history.failed,
                    longDateFormat,
                    timeFormat,
                    { includeSeconds: true }
                  ),
                })
              : translate('GrabbedAt', {
                  date: formatDateTime(
                    history.grabbed,
                    longDateFormat,
                    timeFormat,
                    { includeSeconds: true }
                  ),
                })
          }`}
        />
      ) : null}

      {isBlocklisted ? (
        <Icon
          name={icons.BLOCKLIST}
          kind={kinds.DANGER}
          title={
            history?.failed
              ? `${translate('BlocklistedAt', {
                  date: formatDateTime(
                    history.failed,
                    longDateFormat,
                    timeFormat,
                    { includeSeconds: true }
                  ),
                })}`
              : translate('Blocklisted')
          }
        />
      ) : null}

      {rejections.length ? (
        <Popover
          className={styles.popover}
          anchor={<Icon name={icons.DANGER} kind={kinds.DANGER} size={16} />}
          title={translate('ReleaseRejected')}
          body={
            <ul>
              {rejections.map((rejection, index) => {
                return <li key={index}>{rejection.message}</li>;
              })}
            </ul>
          }
          position={tooltipPositions.BOTTOM}
        />
      ) : null}
    </>
  );
}

export default ReleaseStatusIcons;
