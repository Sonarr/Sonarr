import React from 'react';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import SeriesIndexTags from 'Series/Index/SeriesIndexTags';

interface TagsCellProps {
  className: string;
  tags: number[];
}

function TagsCell({ className, tags }: TagsCellProps) {
  return (
    <VirtualTableRowCell className={className}>
      <SeriesIndexTags tags={tags} />
    </VirtualTableRowCell>
  );
}

export default TagsCell;
