import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FieldSet from 'Components/FieldSet';
import PageSectionContent from 'Components/Page/PageSectionContent';
import QualityDefinitionConnector from './QualityDefinitionConnector';
import styles from './QualityDefinitions.css';

class QualityDefinitions extends Component {

  //
  // Render

  render() {
    const {
      items,
      ...otherProps
    } = this.props;

    return (
      <FieldSet legend="Quality Definitions">
        <PageSectionContent
          errorMessage="Unable to load Quality Definitions"
          {...otherProps}
        >
          <div className={styles.header}>
            <div className={styles.quality}>Quality</div>
            <div className={styles.title}>Title</div>
            <div className={styles.sizeLimit}>Size Limit</div>
            <div className={styles.megabytesPerMinute}>Megabytes Per Minute</div>
          </div>

          <div className={styles.definitions}>
            {
              items.map((item) => {
                return (
                  <QualityDefinitionConnector
                    key={item.id}
                    {...item}
                  />
                );
              })
            }
          </div>

          <div className={styles.sizeLimitHelpTextContainer}>
            <div className={styles.sizeLimitHelpText}>
              Limits are automatically adjusted for the series runtime and number of episodes in the file.
            </div>
          </div>
        </PageSectionContent>
      </FieldSet>
    );
  }
}

QualityDefinitions.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  defaultProfile: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default QualityDefinitions;
