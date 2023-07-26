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
      code
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

    // replace blocks in the string surrounded in backticks with <code>
    // if the first character is a backtick then we ignore the first split and start directly with a code block
    if (code) {
      const codeSplit = code.split('`');
      const startIndex = (code.startsWith('`') === true) ? 1 : 0;

      for (let index = startIndex; index < codeSplit.length; index++) {
        if (index % 2 === 1) {
          markdownBlocks.push(<code>{codeSplit[index]}</code>);
        } else {
          markdownBlocks.push(codeSplit[index]);
        }
      }
    }
    return <span className={className}>{markdownBlocks}</span>;
  }
}

InlineMarkdown.propTypes = {
  className: PropTypes.string,
  data: PropTypes.string,
  code: PropTypes.string
};

export default InlineMarkdown;
