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
    this._buttonRef = {};
    this._contentRef = {};

    this.state = {
      term: props.id,
      isOpen: false
    };
  }

  //
  // Control

  _addListener() {
    window.addEventListener('click', this.onWindowClick);
  }

  _removeListener() {
    window.removeEventListener('click', this.onWindowClick);
  }

  //
  // Listeners

  onWindowClick = (event) => {
    const button = ReactDOM.findDOMNode(this._buttonRef.current);
    const content = ReactDOM.findDOMNode(this._contentRef.current);

    if (!button || !content) {
      return;
    }

    if (
      !button.contains(event.target) &&
      !content.contains(event.target) &&
      this.state.isOpen
    ) {
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
      isQueued,
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
        renderTarget={
          (ref) => {
            this._buttonRef = ref;

            return (
              <div ref={ref}>
                <Link
                  className={styles.button}
                  component="div"
                  onPress={this.onPress}
                >
                  {
                    isLookingUpSeries && isQueued && !isPopulated ?
                      <LoadingIndicator
                        className={styles.loading}
                        size={20}
                      /> :
                      null
                  }

                  {
                    isPopulated && selectedSeries && isExistingSeries ?
                      <Icon
                        className={styles.warningIcon}
                        name={icons.WARNING}
                        kind={kinds.WARNING}
                      /> :
                      null
                  }

                  {
                    isPopulated && selectedSeries ?
                      <ImportSeriesTitle
                        title={selectedSeries.title}
                        year={selectedSeries.year}
                        network={selectedSeries.network}
                        isExistingSeries={isExistingSeries}
                      /> :
                      null
                  }

                  {
                    isPopulated && !selectedSeries ?
                      <div className={styles.noMatches}>
                        <Icon
                          className={styles.warningIcon}
                          name={icons.WARNING}
                          kind={kinds.WARNING}
                        />

                No match found!
                      </div> :
                      null
                  }

                  {
                    !isFetching && !!error ?
                      <div>
                        <Icon
                          className={styles.warningIcon}
                          title={errorMessage}
                          name={icons.WARNING}
                          kind={kinds.WARNING}
                        />

                Search failed, please try again later.
                      </div> :
                      null
                  }

                  <div className={styles.dropdownArrowContainer}>
                    <Icon
                      name={icons.CARET_DOWN}
                    />
                  </div>
                </Link>
              </div>
            );
          }
        }
        renderElement={
          (ref) => {
            this._contentRef = ref;

            if (!this.state.isOpen) {
              return;
            }

            return (
              <div
                ref={ref}
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
            );
          }
        }
      />
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
  isQueued: PropTypes.bool.isRequired,
  isLookingUpSeries: PropTypes.bool.isRequired,
  onSearchInputChange: PropTypes.func.isRequired,
  onSeriesSelect: PropTypes.func.isRequired
};

ImportSeriesSelectSeries.defaultProps = {
  isFetching: true,
  isPopulated: false,
  items: [],
  isQueued: true
};

export default ImportSeriesSelectSeries;
