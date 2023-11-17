import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FieldSet from 'Components/FieldSet';
import PageSectionContent from 'Components/Page/PageSectionContent';
import translate from 'Utilities/String/translate';
import QualityDefinitionConnector from './QualityDefinitionConnector';
import styles from './QualityDefinitions.css';

class QualityDefinitions extends Component {

  //
  // Render

  render() {
    const {
      items,
      advancedSettings,
      ...otherProps
    } = this.props;

    return (
      <FieldSet legend={translate('QualityDefinitions')}>
        <PageSectionContent
          errorMessage={translate('QualityDefinitionsLoadError')}
          {...otherProps}
        >
          <div className={styles.header}>
            <div className={styles.quality}>
              {translate('Quality')}
            </div>
            <div className={styles.title}>
              {translate('Title')}
            </div>
            <div className={styles.sizeLimit}>
              {translate('SizeLimit')}
            </div>

            {
              advancedSettings ?
                <div className={styles.megabytesPerMinute}>
                  {translate('MegabytesPerMinute')}
                </div> :
                null
            }
          </div>

          <div className={styles.definitions}>
            {
              items.map((item) => {
                return (
                  <QualityDefinitionConnector
                    key={item.id}
                    {...item}
                    advancedSettings={advancedSettings}
                  />
                );
              })
            }
          </div>

          <div className={styles.sizeLimitHelpTextContainer}>
            <div className={styles.sizeLimitHelpText}>
              {translate('QualityLimitsSeriesRuntimeHelpText')}
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
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  advancedSettings: PropTypes.bool.isRequired
};

export default QualityDefinitions;
