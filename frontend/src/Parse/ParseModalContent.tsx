import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons } from 'Helpers/Props';
import { clear, fetch } from 'Store/Actions/parseActions';
import { InputChanged } from 'typings/inputs';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import ParseResult from './ParseResult';
import parseStateSelector from './parseStateSelector';
import styles from './ParseModalContent.css';

interface ParseModalContentProps {
  onModalClose: () => void;
}

function ParseModalContent(props: ParseModalContentProps) {
  const { onModalClose } = props;
  const { isFetching, error, item } = useSelector(parseStateSelector());

  const [title, setTitle] = useState('');
  const dispatch = useDispatch();

  const onInputChange = useCallback(
    ({ value }: InputChanged<string>) => {
      const trimmedValue = value.trim();

      setTitle(value);

      if (trimmedValue === '') {
        dispatch(clear());
      } else {
        dispatch(fetch({ title: trimmedValue }));
      }
    },
    [setTitle, dispatch]
  );

  const onClearPress = useCallback(() => {
    setTitle('');
    dispatch(clear());
  }, [setTitle, dispatch]);

  useEffect(
    () => {
      return () => {
        dispatch(clear());
      };
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('TestParsing')}</ModalHeader>

      <ModalBody>
        <div className={styles.inputContainer}>
          <div className={styles.inputIconContainer}>
            <Icon name={icons.PARSE} size={20} />
          </div>

          <TextInput
            className={styles.input}
            name="title"
            value={title}
            placeholder="eg. Series.Title.S01E05.720p.HDTV-RlsGroup"
            autoFocus={true}
            onChange={onInputChange}
          />

          <Button className={styles.clearButton} onPress={onClearPress}>
            <Icon name={icons.REMOVE} size={20} />
          </Button>
        </div>

        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && !!error ? (
          <div className={styles.message}>
            <div className={styles.helpText}>
              {translate('ParseModalErrorParsing')}
            </div>
            <div>{getErrorMessage(error)}</div>
          </div>
        ) : null}

        {!isFetching && title && !error && !item.parsedEpisodeInfo ? (
          <div className={styles.message}>
            {translate('ParseModalUnableToParse')}
          </div>
        ) : null}

        {!isFetching && !error && item.parsedEpisodeInfo ? (
          <ParseResult item={item} />
        ) : null}

        {title ? null : (
          <div className={styles.message}>
            <div className={styles.helpText}>
              {translate('ParseModalHelpText')}
            </div>
            <div>{translate('ParseModalHelpTextDetails')}</div>
          </div>
        )}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default ParseModalContent;
