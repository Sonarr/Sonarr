import React, { ReactElement } from 'react';
import Link from 'Components/Link/Link';

interface InlineMarkdownProps {
  className?: string;
  data?: string;
  blockClassName?: string;
}

function InlineMarkdown(props: InlineMarkdownProps) {
  const { className, data, blockClassName } = props;

  // For now only replace links or code blocks (not both)
  const markdownBlocks: (ReactElement | string)[] = [];

  if (data) {
    const linkRegex = RegExp(/\[(.+?)\]\((.+?)\)/g);

    let endIndex = 0;
    let match = null;

    while ((match = linkRegex.exec(data)) !== null) {
      if (match.index > endIndex) {
        markdownBlocks.push(data.substr(endIndex, match.index - endIndex));
      }

      markdownBlocks.push(
        <Link key={match.index} to={match[2]}>
          {match[1]}
        </Link>
      );
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

      markdownBlocks.push(
        <code
          key={`code-${match.index}`}
          className={blockClassName ?? undefined}
        >
          {match[0].substring(1, match[0].length - 1)}
        </code>
      );
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

export default InlineMarkdown;
