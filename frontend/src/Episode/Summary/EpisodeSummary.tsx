import React, { useCallback, useEffect } from 'react';
import { useDispatch } from 'react-redux';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import Episode from 'Episode/Episode';
import useEpisode, { EpisodeEntities } from 'Episode/useEpisode';
import useEpisodeFile from 'EpisodeFile/useEpisodeFile';
import { icons, kinds, sizes } from 'Helpers/Props';
import Series from 'Series/Series';
import useSeries from 'Series/useSeries';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import {
  deleteEpisodeFile,
  fetchEpisodeFile,
} from 'Store/Actions/episodeFileActions';
import translate from 'Utilities/String/translate';
import EpisodeAiring from './EpisodeAiring';
import EpisodeFileRow from './EpisodeFileRow';
import styles from './EpisodeSummary.css';

const COLUMNS: Column[] = [
  {
    name: 'path',
    label: () => translate('Path'),
    isSortable: false,
    isVisible: true,
  },
  {
    name: 'size',
    label: () => translate('Size'),
    isSortable: false,
    isVisible: true,
  },
  {
    name: 'languages',
    label: () => translate('Languages'),
    isSortable: false,
    isVisible: true,
  },
  {
    name: 'quality',
    label: () => translate('Quality'),
    isSortable: false,
    isVisible: true,
  },
  {
    name: 'customFormats',
    label: () => translate('Formats'),
    isSortable: false,
    isVisible: true,
  },
  {
    name: 'customFormatScore',
    label: React.createElement(Icon, {
      name: icons.SCORE,
      title: () => translate('CustomFormatScore'),
    }),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'actions',
    label: '',
    isSortable: false,
    isVisible: true,
  },
];

interface EpisodeSummaryProps {
  seriesId: number;
  episodeId: number;
  episodeEntity: EpisodeEntities;
  episodeFileId?: number;
}

function EpisodeSummary(props: EpisodeSummaryProps) {
  const { seriesId, episodeId, episodeEntity, episodeFileId } = props;

  const dispatch = useDispatch();

  const { qualityProfileId, network } = useSeries(seriesId) as Series;

  const { airDateUtc, overview } = useEpisode(
    episodeId,
    episodeEntity
  ) as Episode;

  const {
    path,
    mediaInfo,
    size,
    languages,
    quality,
    qualityCutoffNotMet,
    customFormats,
    customFormatScore,
  } = useEpisodeFile(episodeFileId) || {};

  const handleDeleteEpisodeFile = useCallback(() => {
    dispatch(
      deleteEpisodeFile({
        id: episodeFileId,
        episodeEntity,
      })
    );
  }, [episodeFileId, episodeEntity, dispatch]);

  useEffect(() => {
    if (episodeFileId && !path) {
      dispatch(fetchEpisodeFile({ id: episodeFileId }));
    }
  }, [episodeFileId, path, dispatch]);

  const hasOverview = !!overview;

  return (
    <div>
      <div>
        <span className={styles.infoTitle}>{translate('Airs')}</span>

        <EpisodeAiring airDateUtc={airDateUtc} network={network} />
      </div>

      <div>
        <span className={styles.infoTitle}>{translate('QualityProfile')}</span>

        <Label kind={kinds.PRIMARY} size={sizes.MEDIUM}>
          <QualityProfileNameConnector qualityProfileId={qualityProfileId} />
        </Label>
      </div>

      <div className={styles.overview}>
        {hasOverview ? overview : translate('NoEpisodeOverview')}
      </div>

      {path ? (
        <Table columns={COLUMNS}>
          <TableBody>
            <EpisodeFileRow
              path={path}
              size={size!}
              languages={languages!}
              quality={quality!}
              qualityCutoffNotMet={qualityCutoffNotMet!}
              customFormats={customFormats!}
              customFormatScore={customFormatScore!}
              mediaInfo={mediaInfo!}
              columns={COLUMNS}
              onDeleteEpisodeFile={handleDeleteEpisodeFile}
            />
          </TableBody>
        </Table>
      ) : null}
    </div>
  );
}

export default EpisodeSummary;
