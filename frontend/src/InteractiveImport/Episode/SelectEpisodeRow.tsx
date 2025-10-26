import React, { useCallback } from 'react';
import { useSelect } from 'App/Select/SelectContext';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRowButton from 'Components/Table/TableRowButton';
import Episode from 'Episode/Episode';
import { SelectStateInputProps } from 'typings/props';

interface SelectEpisodeRowProps {
  id: number;
  episodeNumber: number;
  absoluteEpisodeNumber: number | undefined;
  title: string;
  airDate: string;
  isAnime: boolean;
  isSelected?: boolean;
}

function SelectEpisodeRow({
  id,
  episodeNumber,
  absoluteEpisodeNumber,
  title,
  airDate,
  isAnime,
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

  return (
    <TableRowButton onPress={handlePress}>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={handleSelectedChange}
      />

      <TableRowCell>
        {episodeNumber}
        {isAnime ? ` (${absoluteEpisodeNumber})` : ''}
      </TableRowCell>

      <TableRowCell>{title}</TableRowCell>

      <TableRowCell>{airDate}</TableRowCell>
    </TableRowButton>
  );
}

export default SelectEpisodeRow;
