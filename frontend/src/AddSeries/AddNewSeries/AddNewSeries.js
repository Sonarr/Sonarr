import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { icons, kinds } from 'Helpers/Props';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import AddNewSeriesSearchResultConnector from './AddNewSeriesSearchResultConnector';
import styles from './AddNewSeries.css';

class AddNewSeries extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      term: props.term || '',
      isFetching: false
    };
  }

  componentDidMount() {
    const term = this.state.term;

    if (term) {
      this.props.onSeriesLookupChange(term);
    }
  }

  componentDidUpdate(prevProps) {
    const {
      term,
      isFetching
    } = this.props;

    if (term && term !== prevProps.term) {
      this.setState({
        term,
        isFetching: true
      });
      this.props.onSeriesLookupChange(term);
    } else if (isFetching !== prevProps.isFetching) {
      this.setState({
        isFetching
      });
    }
  }

  //
  // Listeners

  onSearchInputChange = ({ value }) => {
    const hasValue = !!value.trim();

    this.setState({ term: value, isFetching: hasValue }, () => {
      if (hasValue) {
        this.props.onSeriesLookupChange(value);
      } else {
        this.props.onClearSeriesLookup();
      }
    });
  };

  onClearSeriesLookupPress = () => {
    this.setState({ term: '' });
    this.props.onClearSeriesLookup();
  };

  //
  // Render

  render() {
    const {
      error,
      items,
      hasExistingSeries
    } = this.props;

    const term = this.state.term;
    const isFetching = this.state.isFetching;

    return (
      <PageContent title={translate('AddNewSeries')}>
        <PageContentBody>
          <div className={styles.searchContainer}>
            <div className={styles.searchIconContainer}>
              <Icon
                name={icons.SEARCH}
                size={20}
              />
            </div>

            <TextInput
              className={styles.searchInput}
              name="seriesLookup"
              value={term}
              placeholder="eg. Breaking Bad, tvdb:####"
              autoFocus={true}
              onChange={this.onSearchInputChange}
            />

            <Button
              className={styles.clearLookupButton}
              onPress={this.onClearSeriesLookupPress}
            >
              <Icon
                name={icons.REMOVE}
                size={20}
              />
            </Button>
          </div>

          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error ?
              <div className={styles.message}>
                <div className={styles.helpText}>
                  {translate('AddNewSeriesError')}
                </div>

                <Alert kind={kinds.DANGER}>{getErrorMessage(error)}</Alert>
              </div> : null
          }

          {
            !isFetching && !error && !!items.length &&
              <div className={styles.searchResults}>
                {
                  items.map((item) => {
                    return (
                      <AddNewSeriesSearchResultConnector
                        key={item.tvdbId}
                        {...item}
                      />
                    );
                  })
                }
              </div>
          }

          {
            !isFetching && !error && !items.length && !!term &&
              <div className={styles.message}>
                <div className={styles.noResults}>{translate('CouldNotFindResults', { term })}</div>
                <div>{translate('SearchByTvdbId')}</div>
                <div>
                  <Link to="https://wiki.servarr.com/sonarr/faq#why-cant-i-add-a-new-series-when-i-know-the-tvdb-id">
                    {translate('WhyCantIFindMyShow')}
                  </Link>
                </div>
              </div>
          }

          {
            term ?
              null :
              <div className={styles.message}>
                <div className={styles.helpText}>
                  {translate('AddNewSeriesHelpText')}
                </div>
                <div>{translate('SearchByTvdbId')}</div>
              </div>
          }

          {
            !term && !hasExistingSeries ?
              <div className={styles.message}>
                <div className={styles.noSeriesText}>
                  {translate('NoSeriesHaveBeenAdded')}
                </div>
                <div>
                  <Button
                    to="/add/import"
                    kind={kinds.PRIMARY}
                  >
                    {translate('ImportExistingSeries')}
                  </Button>
                </div>
              </div> :
              null
          }

          <div />
        </PageContentBody>
      </PageContent>
    );
  }
}

AddNewSeries.propTypes = {
  term: PropTypes.string,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isAdding: PropTypes.bool.isRequired,
  addError: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  hasExistingSeries: PropTypes.bool.isRequired,
  onSeriesLookupChange: PropTypes.func.isRequired,
  onClearSeriesLookup: PropTypes.func.isRequired
};

export default AddNewSeries;
