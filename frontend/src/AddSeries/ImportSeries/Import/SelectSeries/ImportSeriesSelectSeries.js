import PropTypes from 'prop-types';
import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import TetherComponent from 'react-tether';
import { icons, kinds } from 'Helpers/Props';
import Icon from 'Components/Icon';
import FormInputButton from 'Components/Form/FormInputButton';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import TextInput from 'Components/Form/TextInput';
import ImportSeriesSearchResultConnector from './ImportSeriesSearchResultConnector';
import ImportSeriesTitle from './ImportSeriesTitle';
import styles from './ImportSeriesSelectSeries.css';

const tetherOptions = {
  skipMoveElement: true,
  constraints: [
    {
      to: 'window',
      attachment: 'together',
      pin: true
    }
  ],
  attachment: 'top center',
  targetAttachment: 'bottom center'
};

class ImportSeriesSelectSeries extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._seriesLookupTimeout = null;

    this.state = {
      term: props.id,
      isOpen: false
    };
  }

  //
  // Control

  _setButtonRef = (ref) => {
    this._buttonRef = ref;
  }

  _setContentRef = (ref) => {
    this._contentRef = ref;
  }

  _addListener() {
    window.addEventListener('click', this.onWindowClick);
  }

  _removeListener() {
    window.removeEventListener('click', this.onWindowClick);
  }

  //
  // Listeners

  onWindowClick = (event) => {
    const button = ReactDOM.findDOMNode(this._buttonRef);
    const content = ReactDOM.findDOMNode(this._contentRef);

    if (!button) {
      return;
    }

    if (!button.contains(event.target) && content && !content.contains(event.target) && this.state.isOpen) {
      this.setState({ isOpen: false });
      this._removeListener();
    }
  }

  onPress = () => {
    if (this.state.isOpen) {
      this._removeListener();
    } else {
      this._addListener();
    }

    this.setState({ isOpen: !this.state.isOpen });
  }

  onSearchInputChange = ({ value }) => {
    if (this._seriesLookupTimeout) {
      clearTimeout(this._seriesLookupTimeout);
    }

    this.setState({ term: value }, () => {
      this._seriesLookupTimeout = setTimeout(() => {
        this.props.onSearchInputChange(value);
      }, 200);
    });
  }

  onRefreshPress = () => {
    this.props.onSearchInputChange(this.state.term);
  }

  onSeriesSelect = (tvdbId) => {
    this.setState({ isOpen: false });

    this.props.onSeriesSelect(tvdbId);
  }

  //
  // Render

  render() {
    const {
      selectedSeries,
      isExistingSeries,
      isFetching,
      isPopulated,
      error,
      items,
      queued,
      isLookingUpSeries
    } = this.props;

    const errorMessage = error &&
      error.responseJSON &&
      error.responseJSON.message;

    return (
      <TetherComponent
        classes={{
          element: styles.tether
        }}
        {...tetherOptions}
      >
        <Link
          ref={this._setButtonRef}
          className={styles.button}
          component="div"
          onPress={this.onPress}
        >
          {
            isLookingUpSeries && queued && !isPopulated &&
              <LoadingIndicator
                className={styles.loading}
                size={20}
              />
          }

          {
            isPopulated && selectedSeries && isExistingSeries &&
              <Icon
                className={styles.warningIcon}
                name={icons.WARNING}
                kind={kinds.WARNING}
              />
          }

          {
            isPopulated && selectedSeries &&
              <ImportSeriesTitle
                title={selectedSeries.title}
                year={selectedSeries.year}
                network={selectedSeries.network}
                isExistingSeries={isExistingSeries}
              />
          }

          {
            isPopulated && !selectedSeries &&
              <div>
                <Icon
                  className={styles.warningIcon}
                  name={icons.WARNING}
                  kind={kinds.WARNING}
                />

                No match found!
              </div>
          }

          {
            !isFetching && !!error &&
              <div>
                <Icon
                  className={styles.warningIcon}
                  title={errorMessage}
                  name={icons.WARNING}
                  kind={kinds.WARNING}
                />

                Search failed, please try again later.
              </div>
          }

          <div className={styles.dropdownArrowContainer}>
            <Icon
              name={icons.CARET_DOWN}
            />
          </div>
        </Link>

        {
          this.state.isOpen &&
            <div
              ref={this._setContentRef}
              className={styles.contentContainer}
            >
              <div className={styles.content}>
                <div className={styles.searchContainer}>
                  <div className={styles.searchIconContainer}>
                    <Icon name={icons.SEARCH} />
                  </div>

                  <TextInput
                    className={styles.searchInput}
                    name={`${name}_textInput`}
                    value={this.state.term}
                    onChange={this.onSearchInputChange}
                  />

                  <FormInputButton
                    kind={kinds.DEFAULT}
                    spinnerIcon={icons.REFRESH}
                    canSpin={true}
                    isSpinning={isFetching}
                    onPress={this.onRefreshPress}
                  >
                    <Icon name={icons.REFRESH} />
                  </FormInputButton>
                </div>

                <div className={styles.results}>
                  {
                    items.map((item) => {
                      return (
                        <ImportSeriesSearchResultConnector
                          key={item.tvdbId}
                          tvdbId={item.tvdbId}
                          title={item.title}
                          year={item.year}
                          network={item.network}
                          onPress={this.onSeriesSelect}
                        />
                      );
                    })
                  }
                </div>
              </div>
            </div>
        }
      </TetherComponent>
    );
  }
}

ImportSeriesSelectSeries.propTypes = {
  id: PropTypes.string.isRequired,
  selectedSeries: PropTypes.object,
  isExistingSeries: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  queued: PropTypes.bool.isRequired,
  isLookingUpSeries: PropTypes.bool.isRequired,
  onSearchInputChange: PropTypes.func.isRequired,
  onSeriesSelect: PropTypes.func.isRequired
};

ImportSeriesSelectSeries.defaultProps = {
  isFetching: true,
  isPopulated: false,
  items: [],
  queued: true
};

export default ImportSeriesSelectSeries;
