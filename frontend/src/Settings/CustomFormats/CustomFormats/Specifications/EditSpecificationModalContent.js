import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './EditSpecificationModalContent.css';

function EditSpecificationModalContent(props) {
  const {
    advancedSettings,
    item,
    onInputChange,
    onFieldChange,
    onCancelPress,
    onSavePress,
    onDeleteSpecificationPress,
    ...otherProps
  } = props;

  const {
    id,
    implementationName,
    name,
    negate,
    required,
    fields
  } = item;

  return (
    <ModalContent onModalClose={onCancelPress}>
      <ModalHeader>
        {id ? translate('EditConditionImplementation', { implementationName }) : translate('AddConditionImplementation', { implementationName })}
      </ModalHeader>

      <ModalBody>
        <Form
          {...otherProps}
        >
          {
            fields && fields.some((x) => x.label === translate('CustomFormatsSpecificationRegularExpression')) &&
              <Alert kind={kinds.INFO}>
                <div>
                  <InlineMarkdown data={translate('ConditionUsingRegularExpressions')} />
                </div>
                <div>
                  <InlineMarkdown data={translate('RegularExpressionsTutorialLink')} />
                </div>
                <div>
                  <InlineMarkdown data={translate('RegularExpressionsCanBeTested')} />
                </div>
              </Alert>
          }

          <FormGroup>
            <FormLabel>
              {translate('Name')}
            </FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="name"
              {...name}
              onChange={onInputChange}
            />
          </FormGroup>

          {
            fields && fields.map((field) => {
              return (
                <ProviderFieldFormGroup
                  key={field.name}
                  advancedSettings={advancedSettings}
                  provider="specifications"
                  providerData={item}
                  {...field}
                  onChange={onFieldChange}
                />
              );
            })
          }

          <FormGroup>
            <FormLabel>
              {translate('Negate')}
            </FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="negate"
              {...negate}
              helpText={translate('NegateHelpText', { implementationName })}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>
              {translate('Required')}
            </FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="required"
              {...required}
              helpText={translate('RequiredHelpText', { implementationName })}
              onChange={onInputChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>
      <ModalFooter>
        {
          id &&
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteSpecificationPress}
            >
              {translate('Delete')}
            </Button>
        }

        <Button
          onPress={onCancelPress}
        >
          {translate('Cancel')}
        </Button>

        <SpinnerErrorButton
          isSpinning={false}
          onPress={onSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

EditSpecificationModalContent.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onFieldChange: PropTypes.func.isRequired,
  onCancelPress: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onDeleteSpecificationPress: PropTypes.func
};

export default EditSpecificationModalContent;
