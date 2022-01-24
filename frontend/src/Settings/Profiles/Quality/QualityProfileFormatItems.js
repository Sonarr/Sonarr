import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import Link from 'Components/Link/Link';
import { sizes } from 'Helpers/Props';
import QualityProfileFormatItem from './QualityProfileFormatItem';
import styles from './QualityProfileFormatItems.css';

function calcOrder(profileFormatItems) {
  const items = profileFormatItems.reduce((acc, cur, index) => {
    acc[cur.format] = index;
    return acc;
  }, {});

  return [...profileFormatItems].sort((a, b) => {
    if (b.score !== a.score) {
      return b.score - a.score;
    }
    return a.name > b.name ? 1 : -1;
  }).map((x) => items[x.format]);
}

class QualityProfileFormatItems extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      order: calcOrder(this.props.profileFormatItems)
    };
  }

  //
  // Listeners

  onScoreChange = (formatId, value) => {
    const {
      onQualityProfileFormatItemScoreChange
    } = this.props;

    onQualityProfileFormatItemScoreChange(formatId, value);
    this.reorderItems();
  };

  reorderItems = _.debounce(() => this.setState({ order: calcOrder(this.props.profileFormatItems) }), 1000);

  //
  // Render

  render() {
    const {
      profileFormatItems,
      errors,
      warnings
    } = this.props;

    const {
      order
    } = this.state;

    if (profileFormatItems.length < 1) {
      return (
        <div className={styles.addCustomFormatMessage}>
          {'Want more control over which downloads are preferred? Add a'}
          <Link to='/settings/customformats'> Custom Format </Link>
        </div>
      );
    }

    return (
      <FormGroup size={sizes.EXTRA_SMALL}>
        <FormLabel size={sizes.SMALL}>
          Custom Formats
        </FormLabel>

        <div>
          <FormInputHelpText
            text="Sonarr scores each release using the sum of scores for matching custom formats. If a new release would improve the score, at the same or better quality, then Sonarr will grab it."
          />

          {
            errors.map((error, index) => {
              return (
                <FormInputHelpText
                  key={index}
                  text={error.message}
                  isError={true}
                  isCheckInput={false}
                />
              );
            })
          }

          {
            warnings.map((warning, index) => {
              return (
                <FormInputHelpText
                  key={index}
                  text={warning.message}
                  isWarning={true}
                  isCheckInput={false}
                />
              );
            })
          }

          <div className={styles.formats}>
            <div className={styles.headerContainer}>
              <div className={styles.headerTitle}>
                Custom Format
              </div>
              <div className={styles.headerScore}>
                Score
              </div>
            </div>
            {
              order.map((index) => {
                const {
                  format,
                  name,
                  score
                } = profileFormatItems[index];
                return (
                  <QualityProfileFormatItem
                    key={format}
                    formatId={format}
                    name={name}
                    score={score}
                    onScoreChange={this.onScoreChange}
                  />
                );
              })
            }
          </div>
        </div>
      </FormGroup>
    );
  }
}

QualityProfileFormatItems.propTypes = {
  profileFormatItems: PropTypes.arrayOf(PropTypes.object).isRequired,
  errors: PropTypes.arrayOf(PropTypes.object),
  warnings: PropTypes.arrayOf(PropTypes.object),
  onQualityProfileFormatItemScoreChange: PropTypes.func
};

QualityProfileFormatItems.defaultProps = {
  errors: [],
  warnings: []
};

export default QualityProfileFormatItems;
