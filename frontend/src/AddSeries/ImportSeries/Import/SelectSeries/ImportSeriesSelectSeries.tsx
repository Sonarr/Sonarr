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
  useId,
  useRef,
  useState,
} from 'react';
import { useLookupSeries } from 'AddSeries/AddNewSeries/useAddSeries';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
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

const DROPDOWN_MIN_WIDTH = 360;
const DROPDOWN_VIEWPORT_MARGIN = 12;

function handleResultsMouseDown(event: MouseEvent<HTMLDivElement>) {
  event.preventDefault();
}

interface ImportSeriesSelectSeriesProps {
  id: string;
  onInputChange: (input: InputChanged) => void;
  onEditingChange: (isEditing: boolean) => void;
}

function ImportSeriesSelectSeries({
  id,
  onInputChange,
  onEditingChange,
}: ImportSeriesSelectSeriesProps) {
  const importSeriesItem = useImportSeriesItem(id);
  const { selectedSeries, name } = importSeriesItem ?? {};
  const isExistingSeries = useExistingSeries(selectedSeries?.tvdbId);

  const [term, setTerm] = useState(name);
  const [editText, setEditText] = useState('');
  const [isEditing, setIsEditing] = useState(false);
  const [highlightedIndex, setHighlightedIndex] = useState(0);

  const inputRef = useRef<HTMLInputElement>(null);
  const resultsId = useId();

  const query = useDebounce(term, term ? 300 : 0);
  const isCurrentLookupQueueItem = useIsCurrentLookupQueueItem(id);
  const isQueued = useIsCurrentItemQueued(id);

  const { isFetching, isFetched, error, data } = useLookupSeries(
    query,
    isCurrentLookupQueueItem
  );

  const errorMessage = getErrorMessage(error);
  const isLookingUpSeries = isFetching || isQueued;
  const isOpen = isEditing && data.length > 0;

  const { refs, context, floatingStyles } = useFloating({
    middleware: [
      flip({ crossAxis: false, mainAxis: true }),
      shift({ padding: DROPDOWN_VIEWPORT_MARGIN }),
      size({
        apply({ rects, elements }) {
          const maxWidth =
            document.documentElement.clientWidth - DROPDOWN_VIEWPORT_MARGIN * 2;

          elements.floating.style.width = `${Math.min(
            Math.max(rects.reference.width, DROPDOWN_MIN_WIDTH),
            maxWidth
          )}px`;
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

  const handleFindSeriesPress = useCallback(() => {
    setIsEditing(true);
  }, []);

  const handleBlur = useCallback(() => {
    setIsEditing(false);
  }, []);

  const handleInputChange = useCallback(
    ({ value }: InputChanged<string>) => {
      setEditText(value);
      setTerm(value);
      setHighlightedIndex(0);

      if (value) {
        addToLookupQueue(id);
      } else {
        removeFromLookupQueue(id);
      }
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
    if (!isFetched) {
      return;
    }

    const canAutoMatch = !isEditing && !selectedSeries && query === name;

    updateImportSeriesItem({
      id,
      hasSearched: true,
      selectedSeries: canAutoMatch ? data[0] : selectedSeries,
    });

    removeFromLookupQueue(id);
  }, [id, isFetched, data, selectedSeries, isEditing, query, name]);

  useEffect(() => {
    onEditingChange(isEditing);

    return () => {
      onEditingChange(false);
    };
  }, [isEditing, onEditingChange]);

  useEffect(() => {
    setTerm(name);
  }, [name]);

  useEffect(() => {
    setHighlightedIndex(0);
  }, [data]);

  useEffect(() => {
    if (isEditing) {
      inputRef.current?.focus();
      inputRef.current?.select();
    }
  }, [isEditing]);

  const activeRowId =
    isOpen && data.length > 0 ? `${resultsId}_${highlightedIndex}` : undefined;

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

  let seriesControl: React.ReactNode = null;

  if (isEditing) {
    seriesControl = (
      <div
        ref={refs.setReference}
        className={styles.field}
        role="combobox"
        aria-expanded={isOpen}
        aria-haspopup="grid"
        aria-controls={isOpen ? resultsId : undefined}
        aria-activedescendant={activeRowId}
        onKeyDown={handleKeyDown}
        {...getReferenceProps()}
      >
        <Icon className={styles.searchIcon} name={icons.SEARCH} />

        <TextInput
          ref={inputRef}
          className={styles.input}
          name={`${id}_series`}
          value={editText}
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
    );
  } else if (selectedSeries) {
    const { title, year } = selectedSeries;
    const hasYearInTitle = title.includes(String(year));

    seriesControl = (
      <Link
        className={styles.matchedSeries}
        title={translate('ChangeMatch')}
        onPress={handleFindSeriesPress}
      >
        <span className={styles.matchedTitle}>{title}</span>

        {year > 0 && !hasYearInTitle ? (
          <span className={styles.matchedYear}>({year})</span>
        ) : null}

        <Icon className={styles.matchedIcon} name={icons.SEARCH} size={13} />
      </Link>
    );
  } else {
    seriesControl = (
      <Button kind={kinds.DEFAULT} onPress={handleFindSeriesPress}>
        {translate('FindSeries')}
      </Button>
    );
  }

  return (
    <>
      {seriesControl}

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
              role="grid"
              id={resultsId}
              onMouseDown={handleResultsMouseDown}
            >
              {data.map((item, index) => {
                return (
                  <ImportSeriesSearchResult
                    key={item.tvdbId}
                    id={`${resultsId}_${index}`}
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
