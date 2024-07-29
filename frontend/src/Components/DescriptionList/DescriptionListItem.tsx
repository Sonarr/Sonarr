import React from 'react';
import DescriptionListItemDescription, {
  DescriptionListItemDescriptionProps,
} from './DescriptionListItemDescription';
import DescriptionListItemTitle, {
  DescriptionListItemTitleProps,
} from './DescriptionListItemTitle';

interface DescriptionListItemProps {
  className?: string;
  titleClassName?: DescriptionListItemTitleProps['className'];
  descriptionClassName?: DescriptionListItemDescriptionProps['className'];
  title?: DescriptionListItemTitleProps['children'];
  data?: DescriptionListItemDescriptionProps['children'];
}

function DescriptionListItem(props: DescriptionListItemProps) {
  const { className, titleClassName, descriptionClassName, title, data } =
    props;

  return (
    <div className={className}>
      <DescriptionListItemTitle className={titleClassName}>
        {title}
      </DescriptionListItemTitle>

      <DescriptionListItemDescription className={descriptionClassName}>
        {data}
      </DescriptionListItemDescription>
    </div>
  );
}

export default DescriptionListItem;
