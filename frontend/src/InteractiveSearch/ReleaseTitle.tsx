import React, { Fragment } from 'react';

interface ReleaseTitleProps {
  title: string;
}

function ReleaseTitle({ title }: ReleaseTitleProps) {
  const parts = title.split(/([._])/);

  return (
    <>
      {parts.map((part, index) => (
        <Fragment key={index}>
          {part}
          {part === '.' || part === '_' ? <wbr /> : null}
        </Fragment>
      ))}
    </>
  );
}

export default ReleaseTitle;
