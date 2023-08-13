import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';

class InlineMarkdown extends Component {

  //
  // Render

  render() {
    const {
      className,
      data,
      blockClassName
    } = this.props;

    // For now only replace links or code blocks (not both)
    const markdownBlocks = [];
    if (data) {
      const linkRegex = RegExp(/\[(.+?)\]\((.+?)\)/g);

      let endIndex = 0;
      let match = null;

      while ((match = linkRegex.exec(data)) !== null) {
        if (match.index > endIndex) {
          markdownBlocks.push(data.substr(endIndex, match.index - endIndex));
        }

        markdownBlocks.push(<Link key={match.index} to={match[2]}>{match[1]}</Link>);
        endIndex = match.index + match[0].length;
      }

      if (endIndex !== data.length && markdownBlocks.length > 0) {
        markdownBlocks.push(data.substr(endIndex, data.length - endIndex));
      }

      const codeRegex = RegExp(/(?=`)`(?!`)[^`]*(?=`)`(?!`)/g);

      endIndex = 0;
      match = null;
      let matchedCode = false;

      while ((match = codeRegex.exec(data)) !== null) {
        matchedCode = true;

        if (match.index > endIndex) {
          markdownBlocks.push(data.substr(endIndex, match.index - endIndex));
        }

        markdownBlocks.push(<code key={`code-${match.index}`} className={blockClassName ?? null}>{match[0].substring(1, match[0].length - 1)}</code>);
        endIndex = match.index + match[0].length;
      }

      if (endIndex !== data.length && markdownBlocks.length > 0 && matchedCode) {
        markdownBlocks.push(data.substr(endIndex, data.length - endIndex));
      }

      if (markdownBlocks.length === 0) {
        markdownBlocks.push(data);
      }
    }

    return <span className={className}>{markdownBlocks}</span>;
  }
}

InlineMarkdown.propTypes = {
  className: PropTypes.string,
  data: PropTypes.string,
  blockClassName: PropTypes.string
};

export default InlineMarkdown;
