import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, kinds } from 'Helpers/Props';
import Icon from 'Components/Icon';
import CheckInput from 'Components/Form/CheckInput';
import styles from './OrganizePreviewRow.css';

class OrganizePreviewRow extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      id,
      onSelectedChange
    } = this.props;

    onSelectedChange({ id, value: true });
  }

  //
  // Listeners

  onSelectedChange = ({ value, shiftKey }) => {
    const {
      id,
      onSelectedChange
    } = this.props;

    onSelectedChange({ id, value, shiftKey });
  }

  //
  // Render

  render() {
    const {
      id,
      existingPath,
      newPath,
      isSelected
    } = this.props;

    return (
      <div className={styles.row}>
        <CheckInput
          containerClassName={styles.selectedContainer}
          name={id.toString()}
          value={isSelected}
          onChange={this.onSelectedChange}
        />

        <div>
          <div>
            <Icon
              name={icons.SUBTRACT}
              kind={kinds.DANGER}
            />

            <span className={styles.path}>
              {existingPath}
            </span>
          </div>

          <div>
            <Icon
              name={icons.ADD}
              kind={kinds.SUCCESS}
            />

            <span className={styles.path}>
              {newPath}
            </span>
          </div>
        </div>
      </div>
    );
  }
}

OrganizePreviewRow.propTypes = {
  id: PropTypes.number.isRequired,
  existingPath: PropTypes.string.isRequired,
  newPath: PropTypes.string.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

export default OrganizePreviewRow;
