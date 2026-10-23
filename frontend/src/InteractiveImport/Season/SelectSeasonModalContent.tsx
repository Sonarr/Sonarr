import React, { useCallback, useMemo, useState } from 'react';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import { Season } from 'Series/Series';
import { useSingleSeries } from 'Series/useSeries';
import translate from 'Utilities/String/translate';
import SelectSeasonRow from './SelectSeasonRow';

interface SelectSeasonModalContentProps {
  seriesId?: number;
  modalTitle: string;
  selectedSeasonNumbers?: number[];
  onSeasonSelect?(seasonNumber: number): void;
  onSeasonsSelect?(seasonNumbers: number[]): void;
  onModalClose(): void;
}

function SelectSeasonModalContent(props: SelectSeasonModalContentProps) {
  const {
    seriesId,
    modalTitle,
    selectedSeasonNumbers = [],
    onSeasonSelect,
    onSeasonsSelect,
    onModalClose,
  } = props;
  const series = useSingleSeries(seriesId);
  const [selected, setSelected] = useState(selectedSeasonNumbers);
  const seasons = useMemo<Season[]>(() => {
    return series?.seasons.slice(0).reverse() || [];
  }, [series]);

  const handleSeasonPress = useCallback(
    (seasonNumber: number) => {
      if (!onSeasonsSelect) {
        onSeasonSelect?.(seasonNumber);
        return;
      }

      setSelected((current) =>
        current.includes(seasonNumber)
          ? current.filter((s) => s !== seasonNumber)
          : [...current, seasonNumber]
      );
    },
    [onSeasonSelect, onSeasonsSelect]
  );

  const handleSeasonsSelectPress = useCallback(() => {
    onSeasonsSelect?.([...selected].sort((a, b) => a - b));
  }, [selected, onSeasonsSelect]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate(
          onSeasonsSelect
            ? 'SelectSeasonsModalTitle'
            : 'SelectSeasonModalTitle',
          { modalTitle }
        )}
      </ModalHeader>

      <ModalBody>
        {seasons.map((item) => {
          return (
            <SelectSeasonRow
              key={item.seasonNumber}
              seasonNumber={item.seasonNumber}
              isSelected={
                onSeasonsSelect
                  ? selected.includes(item.seasonNumber)
                  : undefined
              }
              onSeasonSelect={handleSeasonPress}
            />
          );
        })}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        {onSeasonsSelect ? (
          <Button
            kind={kinds.SUCCESS}
            isDisabled={!selected.length}
            onPress={handleSeasonsSelectPress}
          >
            {translate('SelectSeasons')}
          </Button>
        ) : null}
      </ModalFooter>
    </ModalContent>
  );
}

export default SelectSeasonModalContent;
