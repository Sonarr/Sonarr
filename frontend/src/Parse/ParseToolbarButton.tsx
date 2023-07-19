import React, { Fragment, useCallback, useState } from 'react';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import { icons } from 'Helpers/Props';
import ParseModal from 'Parse/ParseModal';
import translate from 'Utilities/String/translate';

function ParseToolbarButton() {
  const [isParseModalOpen, setIsParseModalOpen] = useState(false);

  const onOpenParseModalPress = useCallback(() => {
    setIsParseModalOpen(true);
  }, [setIsParseModalOpen]);

  const onParseModalClose = useCallback(() => {
    setIsParseModalOpen(false);
  }, [setIsParseModalOpen]);

  return (
    <Fragment>
      <PageToolbarButton
        label={translate('TestParsing')}
        iconName={icons.PARSE}
        onPress={onOpenParseModalPress}
      />

      <ParseModal isOpen={isParseModalOpen} onModalClose={onParseModalClose} />
    </Fragment>
  );
}

export default ParseToolbarButton;
