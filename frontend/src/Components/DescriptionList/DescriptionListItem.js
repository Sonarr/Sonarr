import PropTypes from 'prop-types';
import React, { Component } from 'react';
import DescriptionListItemTitle from './DescriptionListItemTitle';
import DescriptionListItemDescription from './DescriptionListItemDescription';

class DescriptionListItem extends Component {

  //
  // Render

  render() {
    const {
      titleClassName,
      descriptionClassName,
      title,
      data
    } = this.props;

    return (
      <span>
        <DescriptionListItemTitle
          className={titleClassName}
        >
          {title}
        </DescriptionListItemTitle>

        <DescriptionListItemDescription
          className={descriptionClassName}
        >
          {data}
        </DescriptionListItemDescription>
      </span>
    );
  }
}

DescriptionListItem.propTypes = {
  titleClassName: PropTypes.string,
  descriptionClassName: PropTypes.string,
  title: PropTypes.string,
  data: PropTypes.oneOfType([PropTypes.string, PropTypes.number, PropTypes.node])
};

export default DescriptionListItem;
