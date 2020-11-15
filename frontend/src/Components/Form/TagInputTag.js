import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, kinds } from 'Helpers/Props';
import tagShape from 'Helpers/Props/Shapes/tagShape';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import styles from './TagInputTag.css';

class TagInputTag extends Component {

  //
  // Listeners

  onDelete = () => {
    const {
      index,
      tag,
      onDelete
    } = this.props;

    onDelete({
      index,
      id: tag.id
    });
  }

  onEdit = () => {
    const {
      index,
      tag,
      onEdit
    } = this.props;

    onEdit({
      index,
      id: tag.id,
      value: tag.name
    });
  }

  //
  // Render

  render() {
    const {
      tag,
      kind,
      canEdit
    } = this.props;
    return (
      <div
        className={styles.tag}
        tabIndex={-1}
      >
        <Label
          kind={kind}
        >
          <Link
            tabIndex={-1}
            onPress={this.onDelete}
          >

            {tag.name}
          </Link>

          {
            canEdit ?
              <div className={styles.editContainer}>
                <IconButton
                  className={styles.editButton}
                  name={icons.EDIT}
                  size={9}
                  onPress={this.onEdit}
                />
              </div> :
              null
          }
        </Label>
      </div>
    );
  }
}

TagInputTag.propTypes = {
  index: PropTypes.number.isRequired,
  tag: PropTypes.shape(tagShape),
  kind: PropTypes.oneOf(kinds.all).isRequired,
  canEdit: PropTypes.bool.isRequired,
  onDelete: PropTypes.func.isRequired,
  onEdit: PropTypes.func.isRequired
};

export default TagInputTag;
