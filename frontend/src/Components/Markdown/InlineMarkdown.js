import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';

class InlineMarkdown extends Component {

  //
  // Render

  render() {
    const {
      className,
      data
    } = this.props;

    // For now only replace links
    const markdownBlocks = [];
    if (data) {
      const regex = RegExp(/\[(.+?)\]\((.+?)\)/g);

      let endIndex = 0;
      let match = null;
      while ((match = regex.exec(data)) !== null) {
        if (match.index > endIndex) {
          markdownBlocks.push(data.substr(endIndex, match.index - endIndex));
        }
        markdownBlocks.push(<Link key={match.index} to={match[2]}>{match[1]}</Link>);
        endIndex = match.index + match[0].length;
      }

      if (endIndex !== data.length) {
        markdownBlocks.push(data.substr(endIndex, data.length - endIndex));
      }
    }

    return <span className={className}>{markdownBlocks}</span>;
  }
}

InlineMarkdown.propTypes = {
  className: PropTypes.string,
  data: PropTypes.string
};

export default InlineMarkdown;
