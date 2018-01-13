import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds } from 'Helpers/Props';
import { tagShape } from './TagInput';
import styles from './TagInputInput.css';

class TagInputInput extends Component {

  onMouseDown = (event) => {
    event.preventDefault();

    const {
      isFocused,
      onInputContainerPress
    } = this.props;

    if (isFocused) {
      return;
    }

    onInputContainerPress();
  }

  render() {
    const {
      className,
      tags,
      inputProps,
      kind,
      tagComponent: TagComponent,
      onTagDelete
    } = this.props;

    return (
      <div
        className={className}
        component="div"
        onMouseDown={this.onMouseDown}
      >
        {
          tags.map((tag, index) => {
            return (
              <TagComponent
                key={tag.id}
                index={index}
                tag={tag}
                kind={kind}
                isLastTag={index === tags.length - 1}
                onDelete={onTagDelete}
              />
            );
          })
        }

        <input {...inputProps} />
      </div>
    );
  }
}

TagInputInput.propTypes = {
  className: PropTypes.string.isRequired,
  tags: PropTypes.arrayOf(PropTypes.shape(tagShape)).isRequired,
  inputProps: PropTypes.object.isRequired,
  kind: PropTypes.oneOf(kinds.all).isRequired,
  isFocused: PropTypes.bool.isRequired,
  tagComponent: PropTypes.func.isRequired,
  onTagDelete: PropTypes.func.isRequired,
  onInputContainerPress: PropTypes.func.isRequired
};

TagInputInput.defaultProps = {
  className: styles.inputContainer
};

export default TagInputInput;
