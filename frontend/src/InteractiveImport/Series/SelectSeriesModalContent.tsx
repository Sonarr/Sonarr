import React, { useCallback, useMemo, useState } from 'react';
import { useSelector } from 'react-redux';
import TextInput from 'Components/Form/TextInput';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Scroller from 'Components/Scroller/Scroller';
import { scrollDirections } from 'Helpers/Props';
import Series from 'Series/Series';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import SelectSeriesRow from './SelectSeriesRow';
import styles from './SelectSeriesModalContent.css';

interface SelectSeriesModalContentProps {
  modalTitle: string;
  onSeriesSelect(series: Series): void;
  onModalClose(): void;
}

function SelectSeriesModalContent(props: SelectSeriesModalContentProps) {
  const { modalTitle, onSeriesSelect, onModalClose } = props;

  const allSeries: Series[] = useSelector(createAllSeriesSelector());
  const [filter, setFilter] = useState('');

  const onFilterChange = useCallback(
    ({ value }: { value: string }) => {
      setFilter(value);
    },
    [setFilter]
  );

  const onSeriesSelectWrapper = useCallback(
    (seriesId: number) => {
      const series = allSeries.find((s) => s.id === seriesId) as Series;

      onSeriesSelect(series);
    },
    [allSeries, onSeriesSelect]
  );

  const items = useMemo(() => {
    const sorted = [...allSeries].sort((a, b) =>
      a.sortTitle.localeCompare(b.sortTitle)
    );

    return sorted.filter((item) =>
      item.title.toLowerCase().includes(filter.toLowerCase())
    );
  }, [allSeries, filter]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{modalTitle} - Select Series</ModalHeader>

      <ModalBody
        className={styles.modalBody}
        scrollDirection={scrollDirections.NONE}
      >
        <TextInput
          className={styles.filterInput}
          placeholder="Filter series"
          name="filter"
          value={filter}
          autoFocus={true}
          onChange={onFilterChange}
        />

        <Scroller className={styles.scroller} autoFocus={false}>
          {items.map((item) => {
            return (
              <SelectSeriesRow
                key={item.id}
                id={item.id}
                title={item.title}
                onSeriesSelect={onSeriesSelectWrapper}
              />
            );
          })}
        </Scroller>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>Cancel</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SelectSeriesModalContent;
