import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds } from 'Helpers/Props';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import { tagShape } from './TagInput';

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

  //
  // Render

  render() {
    const {
      tag,
      kind
    } = this.props;

    return (
      <Link onPress={this.onDelete}>
        <Label kind={kind}>
          {tag.name}
        </Label>
      </Link>
    );
  }
}

TagInputTag.propTypes = {
  index: PropTypes.number.isRequired,
  tag: PropTypes.shape(tagShape),
  kind: PropTypes.oneOf(kinds.all).isRequired,
  onDelete: PropTypes.func.isRequired
};

export default TagInputTag;
