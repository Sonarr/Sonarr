import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import SelectInput from 'Components/Form/SelectInput';
import styles from './TablePager.css';

class TablePager extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isShowingPageSelect: false
    };
  }

  //
  // Listeners

  onOpenPageSelectClick = () => {
    this.setState({ isShowingPageSelect: true });
  }

  onPageSelect = ({ value: page }) => {
    this.setState({ isShowingPageSelect: false });
    this.props.onPageSelect(parseInt(page));
  }

  onPageSelectBlur = () => {
    this.setState({ isShowingPageSelect: false });
  }

  //
  // Render

  render() {
    const {
      page,
      totalPages,
      totalRecords,
      isFetching,
      onFirstPagePress,
      onPreviousPagePress,
      onNextPagePress,
      onLastPagePress
    } = this.props;

    const isShowingPageSelect = this.state.isShowingPageSelect;
    const pages = Array.from(new Array(totalPages), (x, i) => {
      const pageNumber = i + 1;

      return {
        key: pageNumber,
        value: pageNumber
      };
    });

    if (!page) {
      return null;
    }

    const isFirstPage = page === 1;
    const isLastPage = page === totalPages;

    return (
      <div className={styles.pager}>
        <div className={styles.loadingContainer}>
          {
            isFetching &&
              <LoadingIndicator
                className={styles.loading}
                size={20}
              />
          }
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
              {
                !isShowingPageSelect &&
                  <Link
                    isDisabled={totalPages === 1}
                    onPress={this.onOpenPageSelectClick}
                  >
                    {page} / {totalPages}
                  </Link>
              }

              {
                isShowingPageSelect &&
                  <SelectInput
                    className={styles.pageSelect}
                    name="pageSelect"
                    value={page}
                    values={pages}
                    autoFocus={true}
                    onChange={this.onPageSelect}
                    onBlur={this.onPageSelectBlur}
                  />
              }
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
            Total records: {totalRecords}
          </div>
        </div>
      </div>
    );
  }

}

TablePager.propTypes = {
  page: PropTypes.number,
  totalPages: PropTypes.number,
  totalRecords: PropTypes.number,
  isFetching: PropTypes.bool,
  onFirstPagePress: PropTypes.func.isRequired,
  onPreviousPagePress: PropTypes.func.isRequired,
  onNextPagePress: PropTypes.func.isRequired,
  onLastPagePress: PropTypes.func.isRequired,
  onPageSelect: PropTypes.func.isRequired
};

export default TablePager;
