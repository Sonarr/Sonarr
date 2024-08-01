import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Measure from 'Components/Measure';
import { icons, kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import QualityProfileItemDragPreview from './QualityProfileItemDragPreview';
import QualityProfileItemDragSource from './QualityProfileItemDragSource';
import styles from './QualityProfileItems.css';

class QualityProfileItems extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      defaultHeight: 0,
      editGroupsHeight: 0,
      editSizesHeight: 0
    };
  }

  //
  // Listeners

  onMeasure = ({ height }) => {
    const heightKey = `${this.props.mode}Height`;

    this.setState({
      [heightKey]: height
    });
  };

  onEditGroupsPress = () => {
    this.props.onChangeMode('editGroups');
  };

  onEditSizesPress = () => {
    this.props.onChangeMode('editSizes');
  };

  onDefaultModePress = () => {
    this.props.onChangeMode('default');
  };

  //
  // Render

  render() {
    const {
      mode,
      dropQualityIndex,
      dropPosition,
      qualityProfileItems,
      errors,
      warnings,
      ...otherProps
    } = this.props;

    const isDragging = dropQualityIndex !== null;
    const isDraggingUp = isDragging && dropPosition === 'above';
    const isDraggingDown = isDragging && dropPosition === 'below';
    const height = this.state[`${mode}Height`];

    return (
      <FormGroup size={sizes.EXTRA_SMALL}>
        <FormLabel size={sizes.SMALL}>
          {translate('Qualities')}
        </FormLabel>

        <div>
          <FormInputHelpText
            text={translate('QualitiesHelpText')}
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

          <Button
            className={styles.editGroupsButton}
            kind={kinds.PRIMARY}
            onPress={mode === 'editGroups' ? this.onDefaultModePress : this.onEditGroupsPress}
          >
            <div>
              <Icon
                className={styles.editButtonIcon}
                name={mode === 'editGroups' ? icons.REORDER : icons.GROUP}
              />

              {
                mode === 'editGroups' ? translate('DoneEditingGroups') : translate('EditGroups')
              }
            </div>
          </Button>

          <Button
            className={styles.editSizesButton}
            kind={kinds.PRIMARY}
            onPress={mode === 'editSizes' ? this.onDefaultModePress : this.onEditSizesPress}
          >
            <div>
              <Icon
                className={styles.editButtonIcon}
                name={mode === 'editSizes' ? icons.REORDER : icons.FILE}
              />

              {
                mode === 'editSizes' ? translate('DoneEditingSizes') : translate('EditSizes')
              }
            </div>
          </Button>

          <Measure
            whitelist={['height']}
            includeMargin={false}
            onMeasure={this.onMeasure}
          >
            <div
              className={styles.qualities}
              style={{ height: `${height}px` }}
            >
              {
                qualityProfileItems.map(({ id, name, allowed, quality, items, minSize, maxSize, preferredSize }, index) => {
                  const identifier = quality ? quality.id : id;

                  return (
                    <QualityProfileItemDragSource
                      key={identifier}
                      mode={mode}
                      groupId={id}
                      qualityId={quality && quality.id}
                      name={quality ? quality.name : name}
                      allowed={allowed}
                      items={items}
                      minSize={minSize}
                      maxSize={maxSize}
                      preferredSize={preferredSize}
                      qualityIndex={`${index + 1}`}
                      isInGroup={false}
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
          </Measure>
        </div>
      </FormGroup>
    );
  }
}

QualityProfileItems.propTypes = {
  mode: PropTypes.string.isRequired,
  dragQualityIndex: PropTypes.string,
  dropQualityIndex: PropTypes.string,
  dropPosition: PropTypes.string,
  qualityProfileItems: PropTypes.arrayOf(PropTypes.object).isRequired,
  errors: PropTypes.arrayOf(PropTypes.object),
  warnings: PropTypes.arrayOf(PropTypes.object),
  onChangeMode: PropTypes.func.isRequired
};

QualityProfileItems.defaultProps = {
  errors: [],
  warnings: []
};

export default QualityProfileItems;
