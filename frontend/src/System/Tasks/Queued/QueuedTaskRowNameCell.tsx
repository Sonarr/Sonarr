import React from 'react';
import { useSelector } from 'react-redux';
import { CommandBody } from 'Commands/Command';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import createMultiSeriesSelector from 'Store/Selectors/createMultiSeriesSelector';
import translate from 'Utilities/String/translate';
import styles from './QueuedTaskRowNameCell.css';

export interface QueuedTaskRowNameCellProps {
  commandName: string;
  body: CommandBody;
  clientUserAgent?: string;
}

export default function QueuedTaskRowNameCell(
  props: QueuedTaskRowNameCellProps
) {
  const { commandName, body, clientUserAgent } = props;
  const seriesIds = [...(body.seriesIds ?? [])];

  if (body.seriesId) {
    seriesIds.push(body.seriesId);
  }

  const series = useSelector(createMultiSeriesSelector(seriesIds));
  const sortedSeries = series.sort((a, b) =>
    a.sortTitle.localeCompare(b.sortTitle)
  );

  return (
    <TableRowCell>
      <span className={styles.commandName}>
        {commandName}
        {sortedSeries.length ? (
          <span> - {sortedSeries.map((s) => s.title).join(', ')}</span>
        ) : null}
        {body.seasonNumber ? (
          <span>
            {' '}
            {translate('SeasonNumberToken', {
              seasonNumber: body.seasonNumber,
            })}
          </span>
        ) : null}
      </span>

      {clientUserAgent ? (
        <span
          className={styles.userAgent}
          title={translate('TaskUserAgentTooltip')}
        >
          {translate('From')}: {clientUserAgent}
        </span>
      ) : null}
    </TableRowCell>
  );
}
