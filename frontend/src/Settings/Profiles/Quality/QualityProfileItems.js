import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import QualityProfileItemDragSource from './QualityProfileItemDragSource';
import QualityProfileItemDragPreview from './QualityProfileItemDragPreview';
import styles from './QualityProfileItems.css';

class QualityProfileItems extends Component {

  //
  // Render

  render() {
    const {
      dragIndex,
      dropIndex,
      qualityProfileItems,
      errors,
      warnings,
      ...otherProps
    } = this.props;

    const isDragging = dropIndex !== null;
    const isDraggingUp = isDragging && dropIndex > dragIndex;
    const isDraggingDown = isDragging && dropIndex < dragIndex;

    return (
      <FormGroup>
        <FormLabel>Qualities</FormLabel>
        <div>
          <FormInputHelpText
            text="Qualities higher in the list are more preferred. Only checked qualities are wanted"
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

          <div className={styles.qualities}>
            {
              qualityProfileItems.map(({ allowed, quality }, index) => {
                return (
                  <QualityProfileItemDragSource
                    key={quality.id}
                    qualityId={quality.id}
                    name={quality.name}
                    allowed={allowed}
                    sortIndex={index}
                    isDragging={isDragging}
                    isDraggingUp={isDraggingUp}
                    isDraggingDown={isDraggingDown}
                    {...otherProps}
                  />
                );
              }).reverse()
            }

            <QualityProfileItemDragPreview />
          </div>
        </div>
      </FormGroup>
    );
  }
}

QualityProfileItems.propTypes = {
  dragIndex: PropTypes.number,
  dropIndex: PropTypes.number,
  qualityProfileItems: PropTypes.arrayOf(PropTypes.object).isRequired,
  errors: PropTypes.arrayOf(PropTypes.object),
  warnings: PropTypes.arrayOf(PropTypes.object)
};

QualityProfileItems.defaultProps = {
  errors: [],
  warnings: []
};

export default QualityProfileItems;
