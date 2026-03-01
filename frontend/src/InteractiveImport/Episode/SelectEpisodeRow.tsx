import React, { useCallback } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRowButton from 'Components/Table/TableRowButton';
import Episode from 'Episode/Episode';
import { icons, kinds } from 'Helpers/Props';
import { SelectStateInputProps } from 'typings/props';
import translate from 'Utilities/String/translate';
import Icon from '../../Components/Icon';
import styles from './SelectEpisodeRow.css';

function getWarningMessage(
  unverifiedSceneNumbering: boolean,
  isAnime: boolean,
  absoluteEpisodeNumber: number | undefined
) {
  const messages = [];

  if (unverifiedSceneNumbering) {
    messages.push(translate('SceneNumberNotVerified'));
  }

  if (isAnime && !absoluteEpisodeNumber) {
    messages.push(translate('EpisodeMissingAbsoluteNumber'));
  }

  return messages.join('\n');
}

interface SelectEpisodeRowProps {
  id: number;
  episodeNumber: number;
  absoluteEpisodeNumber: number | undefined;
  title: string;
  airDate: string;
  isAnime: boolean;
  isSelected?: boolean;
  unverifiedSceneNumbering?: boolean;
}

function SelectEpisodeRow({
  id,
  episodeNumber,
  absoluteEpisodeNumber,
  title,
  airDate,
  isAnime,
  unverifiedSceneNumbering = false,
}: SelectEpisodeRowProps) {
  const { toggleSelected, useIsSelected } = useSelect<Episode>();
  const isSelected = useIsSelected(id);

  const handleSelectedChange = useCallback(
    ({ id, value, shiftKey = false }: SelectStateInputProps) => {
      toggleSelected({
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [toggleSelected]
  );

  const handlePress = useCallback(() => {
    handleSelectedChange({ id, value: !isSelected, shiftKey: false });
  }, [id, isSelected, handleSelectedChange]);

  const warningMessage = getWarningMessage(
    unverifiedSceneNumbering,
    isAnime,
    absoluteEpisodeNumber
  );

  return (
    <TableRowButton onPress={handlePress}>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={handleSelectedChange}
      />

      <TableRowCell>
        {episodeNumber}
        {isAnime && !!absoluteEpisodeNumber
          ? ` (${absoluteEpisodeNumber})`
          : ''}
        {warningMessage ? (
          <Icon
            className={styles.warning}
            name={icons.WARNING}
            kind={kinds.WARNING}
            title={warningMessage}
          />
        ) : null}
      </TableRowCell>

      <TableRowCell>{title}</TableRowCell>

      <TableRowCell>{airDate}</TableRowCell>
    </TableRowButton>
  );
}

export default SelectEpisodeRow;
