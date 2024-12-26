import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import { fetchNotificationTemplates } from 'Store/Actions/settingsActions';
import { EnhancedSelectInputChanged } from 'typings/inputs';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput from './EnhancedSelectInput';

function createNotificationTemplatesSelector(includeAny: boolean) {
  return createSelector(
    (state: AppState) => state.settings.notificationTemplates,
    (notificationTemplates) => {
      const { isFetching, isPopulated, error, items } = notificationTemplates;

      const values = items.sort(sortByProp('name')).map((notificationTemplate) => {
        return {
          key: notificationTemplate.id,
          value: notificationTemplate.name,
        };
      });

      if (includeAny) {
        values.unshift({
          key: 0,
          value: `(${translate('Fallback')})`,
        });
      }

      return {
        isFetching,
        isPopulated,
        error,
        values,
      };
    }
  );
}

interface NotificationTemplateSelectInputConnectorProps {
  name: string;
  value: number;
  includeAny?: boolean;
  values: object[];
  onChange: (change: EnhancedSelectInputChanged<number>) => void;
}

function NotificationTemplateSelectInput({
  name,
  value,
  includeAny = false,
  onChange,
}: NotificationTemplateSelectInputConnectorProps) {
  const dispatch = useDispatch();
  const { isFetching, isPopulated, values } = useSelector(
    createNotificationTemplatesSelector(includeAny)
  );

  useEffect(() => {
    if (!isPopulated) {
      dispatch(fetchNotificationTemplates());
    }
  }, [isPopulated, dispatch]);

  return (
    <EnhancedSelectInput
      name={name}
      value={value}
      isFetching={isFetching}
      values={values}
      onChange={onChange}
    />
  );
}

NotificationTemplateSelectInput.defaultProps = {
  includeAny: false,
};

export default NotificationTemplateSelectInput;
