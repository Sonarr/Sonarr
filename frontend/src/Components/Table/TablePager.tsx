import classNames from 'classnames';
import React, { useCallback, useMemo, useState } from 'react';
import SelectInput from 'Components/Form/SelectInput';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { icons } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './TablePager.css';

interface TablePagerProps {
  page?: number;
  totalPages?: number;
  totalRecords?: number;
  isFetching?: boolean;
  onFirstPagePress: () => void;
  onPreviousPagePress: () => void;
  onNextPagePress: () => void;
  onLastPagePress: () => void;
  onPageSelect: (page: number) => void;
}

function TablePager({
  page,
  totalPages,
  totalRecords = 0,
  isFetching,
  onFirstPagePress,
  onPreviousPagePress,
  onNextPagePress,
  onLastPagePress,
  onPageSelect,
}: TablePagerProps) {
  const [isShowingPageSelect, setIsShowingPageSelect] = useState(false);

  const isFirstPage = page === 1;
  const isLastPage = page === totalPages;

  const pages = useMemo(() => {
    return Array.from(new Array(totalPages), (_x, i) => {
      const pageNumber = i + 1;

      return {
        key: pageNumber,
        value: String(pageNumber),
      };
    });
  }, [totalPages]);

  const handleOpenPageSelectClick = useCallback(() => {
    setIsShowingPageSelect(true);
  }, []);

  const handlePageSelect = useCallback(
    ({ value }: InputChanged<number>) => {
      setIsShowingPageSelect(false);
      onPageSelect(value);
    },
    [onPageSelect]
  );

  const handlePageSelectBlur = useCallback(() => {
    setIsShowingPageSelect(false);
  }, []);

  if (!page) {
    return null;
  }

  return (
    <div className={styles.pager}>
      <div className={styles.loadingContainer}>
        {isFetching ? (
          <LoadingIndicator className={styles.loading} size={20} />
        ) : null}
      </div>

      <div className={styles.controlsContainer}>
        <div className={styles.controls}>
          <Link
            className={classNames(
              styles.pageLink,
              isFirstPage && styles.disabledPageButton
            )}
            isDisabled={isFirstPage}
            onPress={onFirstPagePress}
          >
            <Icon name={icons.PAGE_FIRST} />
          </Link>

          <Link
            className={classNames(
              styles.pageLink,
              isFirstPage && styles.disabledPageButton
            )}
            isDisabled={isFirstPage}
            onPress={onPreviousPagePress}
          >
            <Icon name={icons.PAGE_PREVIOUS} />
          </Link>

          <div className={styles.pageNumber}>
            {isShowingPageSelect ? null : (
              <Link
                isDisabled={totalPages === 1}
                onPress={handleOpenPageSelectClick}
              >
                {page} / {totalPages}
              </Link>
            )}

            {isShowingPageSelect ? (
              <SelectInput
                className={styles.pageSelect}
                name="pageSelect"
                value={page}
                values={pages}
                autoFocus={true}
                onChange={handlePageSelect}
                onBlur={handlePageSelectBlur}
              />
            ) : null}
          </div>

          <Link
            className={classNames(
              styles.pageLink,
              isLastPage && styles.disabledPageButton
            )}
            isDisabled={isLastPage}
            onPress={onNextPagePress}
          >
            <Icon name={icons.PAGE_NEXT} />
          </Link>

          <Link
            className={classNames(
              styles.pageLink,
              isLastPage && styles.disabledPageButton
            )}
            isDisabled={isLastPage}
            onPress={onLastPagePress}
          >
            <Icon name={icons.PAGE_LAST} />
          </Link>
        </div>
      </div>

      <div className={styles.recordsContainer}>
        <div className={styles.records}>
          {translate('TotalRecords', { totalRecords })}
        </div>
      </div>
    </div>
  );
}

export default TablePager;
