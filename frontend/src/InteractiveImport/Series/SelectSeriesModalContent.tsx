import { throttle } from 'lodash';
import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { useSelector } from 'react-redux';
import { FixedSizeList as List, ListChildComponentProps } from 'react-window';
import TextInput from 'Components/Form/TextInput';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Scroller from 'Components/Scroller/Scroller';
import Column from 'Components/Table/Column';
import VirtualTableRowButton from 'Components/Table/VirtualTableRowButton';
import { scrollDirections } from 'Helpers/Props';
import Series from 'Series/Series';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import dimensions from 'Styles/Variables/dimensions';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import SelectSeriesModalTableHeader from './SelectSeriesModalTableHeader';
import SelectSeriesRow from './SelectSeriesRow';
import styles from './SelectSeriesModalContent.css';

const columns = [
  {
    name: 'title',
    label: () => translate('Title'),
    isVisible: true,
  },
  {
    name: 'year',
    label: () => translate('Year'),
    isVisible: true,
  },
  {
    name: 'tvdbId',
    label: () => translate('TvdbId'),
    isVisible: true,
  },
  {
    name: 'imdbId',
    label: () => translate('ImdbId'),
    isVisible: true,
  },
];

const bodyPadding = parseInt(dimensions.pageContentBodyPadding);

interface SelectSeriesModalContentProps {
  modalTitle: string;
  onSeriesSelect(series: Series): void;
  onModalClose(): void;
}

interface RowItemData {
  items: Series[];
  columns: Column[];
  onSeriesSelect(seriesId: number): void;
}

const Row: React.FC<ListChildComponentProps<RowItemData>> = ({
  index,
  style,
  data,
}) => {
  const { items, columns, onSeriesSelect } = data;

  if (index >= items.length) {
    return null;
  }

  const series = items[index];

  return (
    <VirtualTableRowButton
      style={{
        display: 'flex',
        justifyContent: 'space-between',
        ...style,
      }}
      onPress={() => onSeriesSelect(series.id)}
    >
      <SelectSeriesRow
        key={series.id}
        id={series.id}
        title={series.title}
        tvdbId={series.tvdbId}
        imdbId={series.imdbId}
        year={series.year}
        columns={columns}
        onSeriesSelect={onSeriesSelect}
      />
    </VirtualTableRowButton>
  );
};

function SelectSeriesModalContent(props: SelectSeriesModalContentProps) {
  const { modalTitle, onSeriesSelect, onModalClose } = props;

  const listRef = useRef<List<RowItemData>>(null);
  const scrollerRef = useRef<HTMLDivElement>(null);
  const allSeries: Series[] = useSelector(createAllSeriesSelector());
  const [filter, setFilter] = useState('');
  const [size, setSize] = useState({ width: 0, height: 0 });
  const windowHeight = window.innerHeight;

  useEffect(() => {
    const current = scrollerRef?.current as HTMLElement;

    if (current) {
      const width = current.clientWidth;
      const height = current.clientHeight;
      const padding = bodyPadding - 5;

      setSize({
        width: width - padding * 2,
        height: height + padding,
      });
    }
  }, [windowHeight, scrollerRef]);

  useEffect(() => {
    const currentScrollerRef = scrollerRef.current as HTMLElement;
    const currentScrollListener = currentScrollerRef;

    const handleScroll = throttle(() => {
      const { offsetTop = 0 } = currentScrollerRef;
      const scrollTop = currentScrollerRef.scrollTop - offsetTop;

      listRef.current?.scrollTo(scrollTop);
    }, 10);

    currentScrollListener.addEventListener('scroll', handleScroll);

    return () => {
      handleScroll.cancel();

      if (currentScrollListener) {
        currentScrollListener.removeEventListener('scroll', handleScroll);
      }
    };
  }, [listRef, scrollerRef]);

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

  const sortedSeries = useMemo(
    () => [...allSeries].sort(sortByProp('sortTitle')),
    [allSeries]
  );

  const items = useMemo(
    () =>
      sortedSeries.filter(
        (item) =>
          item.title.toLowerCase().includes(filter.toLowerCase()) ||
          item.tvdbId.toString().includes(filter) ||
          item.imdbId?.includes(filter)
      ),
    [sortedSeries, filter]
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{modalTitle} - Select Series</ModalHeader>

      <ModalBody
        className={styles.modalBody}
        scrollDirection={scrollDirections.NONE}
      >
        <TextInput
          className={styles.filterInput}
          placeholder={translate('FilterSeriesPlaceholder')}
          name="filter"
          value={filter}
          autoFocus={true}
          onChange={onFilterChange}
        />

        <Scroller
          className={styles.scroller}
          autoFocus={false}
          ref={scrollerRef}
        >
          <SelectSeriesModalTableHeader columns={columns} />
          <List<RowItemData>
            ref={listRef}
            style={{
              width: '100%',
              height: '100%',
              overflow: 'none',
            }}
            width={size.width}
            height={size.height}
            itemCount={items.length}
            itemSize={38}
            itemData={{
              items,
              columns,
              onSeriesSelect: onSeriesSelectWrapper,
            }}
          >
            {Row}
          </List>
        </Scroller>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SelectSeriesModalContent;
