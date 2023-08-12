import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import translate from 'Utilities/String/translate';

function SeriesTypePopoverContent() {
  return (
    <DescriptionList>
      <DescriptionListItem
        title={translate('Anime')}
        data={translate('AnimeTypeDescription')}
      />

      <DescriptionListItem
        title={translate('Daily')}
        data={translate('DailyTypeDescription')}
      />

      <DescriptionListItem
        title={translate('Standard')}
        data={translate('StandardTypeDescription')}
      />
    </DescriptionList>
  );
}

export default SeriesTypePopoverContent;
