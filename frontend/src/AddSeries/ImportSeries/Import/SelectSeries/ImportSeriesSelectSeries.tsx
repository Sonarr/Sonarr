import {
  autoUpdate,
  flip,
  FloatingPortal,
  shift,
  size,
  useDismiss,
  useFloating,
  useInteractions,
} from '@floating-ui/react';
import React, {
  KeyboardEvent,
  MouseEvent,
  useCallback,
  useEffect,
  useRef,
  useState,
} from 'react';
import { useLookupSeries } from 'AddSeries/AddNewSeries/useAddSeries';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import useDebounce from 'Helpers/Hooks/useDebounce';
import { icons, kinds } from 'Helpers/Props';
import useExistingSeries from 'Series/useExistingSeries';
import { InputChanged } from 'typings/inputs';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import {
  addToLookupQueue,
  removeFromLookupQueue,
  updateImportSeriesItem,
  useImportSeriesItem,
  useIsCurrentItemQueued,
  useIsCurrentLookupQueueItem,
} from '../importSeriesStore';
import ImportSeriesSearchResult from './ImportSeriesSearchResult';
import styles from './ImportSeriesSelectSeries.css';

function handleResultsMouseDown(event: MouseEvent<HTMLDivElement>) {
  event.preventDefault();
}

interface ImportSeriesSelectSeriesProps {
  id: string;
  onInputChange: (input: InputChanged) => void;
}

function ImportSeriesSelectSeries({
  id,
  onInputChange,
}: ImportSeriesSelectSeriesProps) {
  const importSeriesItem = useImportSeriesItem(id);
  const { selectedSeries, name } = importSeriesItem ?? {};
  const isExistingSeries = useExistingSeries(selectedSeries?.tvdbId);

  const [term, setTerm] = useState(name);
  const [editText, setEditText] = useState('');
  const [isEditing, setIsEditing] = useState(false);
  const [highlightedIndex, setHighlightedIndex] = useState(0);

  const inputRef = useRef<HTMLInputElement>(null);

  const query = useDebounce(term, term ? 300 : 0);
  const isCurrentLookupQueueItem = useIsCurrentLookupQueueItem(id);
  const isQueued = useIsCurrentItemQueued(id);

  const { isFetching, isFetched, error, data } = useLookupSeries(
    query,
    isCurrentLookupQueueItem
  );

  const errorMessage = getErrorMessage(error);
  const isLookingUpSeries = isFetching || isQueued;
  const isOpen = isEditing && (isFetching || data.length > 0);

  const { refs, context, floatingStyles } = useFloating({
    middleware: [
      flip({ crossAxis: false, mainAxis: true }),
      shift({ padding: 12 }),
      size({
        apply({ rects, elements }) {
          elements.floating.style.width = `${rects.reference.width}px`;
        },
      }),
    ],
    open: isOpen,
    placement: 'bottom-start',
    whileElementsMounted: autoUpdate,
    onOpenChange: (open) => {
      if (!open) {
        setIsEditing(false);
      }
    },
  });

  const dismiss = useDismiss(context);
  const { getReferenceProps, getFloatingProps } = useInteractions([dismiss]);

  const handleFocus = useCallback(() => {
    setEditText(selectedSeries?.title ?? name ?? '');
    setIsEditing(true);

    if (!selectedSeries) {
      setTerm(name ?? '');
      addToLookupQueue(id);
    }
  }, [id, name, selectedSeries]);

  const handleBlur = useCallback(() => {
    setIsEditing(false);
  }, []);

  const handleInputChange = useCallback(
    ({ value }: InputChanged<string>) => {
      setEditText(value);
      setTerm(value);
      setHighlightedIndex(0);
      addToLookupQueue(id);
    },
    [id]
  );

  const handleSeriesSelect = useCallback(
    (tvdbId: number) => {
      const nextSeries = data.find((item) => item.tvdbId === tvdbId);

      if (!nextSeries) {
        return;
      }

      updateImportSeriesItem({ id, selectedSeries: nextSeries });
      setIsEditing(false);
      inputRef.current?.blur();

      if (nextSeries.seriesType !== 'standard') {
        onInputChange({ name: 'seriesType', value: nextSeries.seriesType });
      }
    },
    [id, data, onInputChange]
  );

  const handleKeyDown = useCallback(
    (event: KeyboardEvent<HTMLDivElement>) => {
      if (event.key === 'ArrowDown') {
        event.preventDefault();
        setHighlightedIndex((index) => Math.min(index + 1, data.length - 1));
      } else if (event.key === 'ArrowUp') {
        event.preventDefault();
        setHighlightedIndex((index) => Math.max(index - 1, 0));
      } else if (event.key === 'Enter') {
        const item = data[highlightedIndex];

        if (item) {
          event.preventDefault();
          handleSeriesSelect(item.tvdbId);
        }
      } else if (event.key === 'Escape') {
        setIsEditing(false);
        inputRef.current?.blur();
      }
    },
    [data, highlightedIndex, handleSeriesSelect]
  );

  useEffect(() => {
    if (isFetched) {
      updateImportSeriesItem({
        id,
        hasSearched: isFetched,
        selectedSeries: data[0],
      });

      removeFromLookupQueue(id);
    }
  }, [id, isFetched, data]);

  useEffect(() => {
    setTerm(name);
  }, [name]);

  useEffect(() => {
    setHighlightedIndex(0);
  }, [data]);

  useEffect(() => {
    if (isEditing) {
      inputRef.current?.select();
    }
  }, [isEditing]);

  const value = isEditing ? editText : selectedSeries?.title ?? '';

  const listboxId = `${id}_series_listbox`;
  const activeOptionId =
    isOpen && data.length > 0 ? `${listboxId}_${highlightedIndex}` : undefined;

  const hasWarning =
    !!error ||
    (isFetched && !selectedSeries) ||
    (!!selectedSeries && isExistingSeries);

  let warningTitle = translate('Existing');

  if (error) {
    warningTitle = errorMessage;
  } else if (isFetched && !selectedSeries) {
    warningTitle = translate('NoMatchFound');
  }

  return (
    <>
      <div
        ref={refs.setReference}
        className={styles.field}
        role="combobox"
        aria-expanded={isOpen}
        aria-haspopup="listbox"
        aria-controls={isOpen ? listboxId : undefined}
        aria-activedescendant={activeOptionId}
        onKeyDown={handleKeyDown}
        {...getReferenceProps()}
      >
        <Icon className={styles.searchIcon} name={icons.SEARCH} />

        <TextInput
          ref={inputRef}
          className={styles.input}
          name={`${id}_series`}
          value={value}
          placeholder={translate('SearchForSeries')}
          onChange={handleInputChange}
          onFocus={handleFocus}
          onBlur={handleBlur}
        />

        <div className={styles.status}>
          {isLookingUpSeries ? (
            <LoadingIndicator className={styles.loadingIndicator} size={20} />
          ) : null}

          {!isLookingUpSeries && hasWarning ? (
            <Icon
              name={icons.WARNING}
              kind={kinds.WARNING}
              title={warningTitle}
            />
          ) : null}
        </div>
      </div>

      {isOpen ? (
        <FloatingPortal id="portal-root">
          <div
            ref={refs.setFloating}
            className={styles.contentContainer}
            style={floatingStyles}
            {...getFloatingProps()}
          >
            <div
              className={styles.results}
              role="listbox"
              id={listboxId}
              onMouseDown={handleResultsMouseDown}
            >
              {data.map((item, index) => {
                return (
                  <ImportSeriesSearchResult
                    key={item.tvdbId}
                    id={`${listboxId}_${index}`}
                    tvdbId={item.tvdbId}
                    title={item.title}
                    year={item.year}
                    network={item.network}
                    isHighlighted={index === highlightedIndex}
                    onPress={handleSeriesSelect}
                  />
                );
              })}
            </div>
          </div>
        </FloatingPortal>
      ) : null}
    </>
  );
}

export default ImportSeriesSelectSeries;
