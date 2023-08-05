import PropTypes from 'prop-types';
import React, { Component, Fragment } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

class QueueOptions extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      includeUnknownSeriesItems: props.includeUnknownSeriesItems
    };
  }

  componentDidUpdate(prevProps) {
    const {
      includeUnknownSeriesItems
    } = this.props;

    if (includeUnknownSeriesItems !== prevProps.includeUnknownSeriesItems) {
      this.setState({
        includeUnknownSeriesItems
      });
    }
  }

  //
  // Listeners

  onOptionChange = ({ name, value }) => {
    this.setState({
      [name]: value
    }, () => {
      this.props.onOptionChange({
        [name]: value
      });
    });
  };

  //
  // Render

  render() {
    const {
      includeUnknownSeriesItems
    } = this.state;

    return (
      <Fragment>
        <FormGroup>
          <FormLabel>{translate('ShowUnknownSeriesItems')}</FormLabel>

          <FormInputGroup
            type={inputTypes.CHECK}
            name="includeUnknownSeriesItems"
            value={includeUnknownSeriesItems}
            helpText={translate('ShownUnknownSeriesItemsHelpText')}
            onChange={this.onOptionChange}
          />
        </FormGroup>
      </Fragment>
    );
  }
}

QueueOptions.propTypes = {
  includeUnknownSeriesItems: PropTypes.bool.isRequired,
  onOptionChange: PropTypes.func.isRequired
};

export default QueueOptions;
