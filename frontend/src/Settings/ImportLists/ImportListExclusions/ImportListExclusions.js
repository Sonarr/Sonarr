import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FieldSet from 'Components/FieldSet';
import IconButton from 'Components/Link/IconButton';
import PageSectionContent from 'Components/Page/PageSectionContent';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TablePager from 'Components/Table/TablePager';
import TableRow from 'Components/Table/TableRow';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditImportListExclusionModalConnector from './EditImportListExclusionModalConnector';
import ImportListExclusion from './ImportListExclusion';
import styles from './ImportListExclusions.css';

const COLUMNS = [
  {
    name: 'title',
    label: () => translate('Title'),
    isVisible: true,
    isSortable: true
  },
  {
    name: 'tvdbid',
    label: () => translate('TvdbId'),
    isVisible: true,
    isSortable: true
  },
  {
    name: 'actions',
    isVisible: true,
    isSortable: false
  }
];

class ImportListExclusions extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddImportListExclusionModalOpen: false
    };
  }

  //
  // Listeners

  onAddImportListExclusionPress = () => {
    this.setState({ isAddImportListExclusionModalOpen: true });
  };

  onModalClose = () => {
    this.setState({ isAddImportListExclusionModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      items,
      isFetching,
      onConfirmDeleteImportListExclusion,
      totalRecords,
      onFirstPagePress,
      isPopulated,
      ...otherProps
    } = this.props;
    const isFetchingOrHasFetched = isFetching && !isPopulated;
    return (
      <FieldSet legend={translate('ImportListExclusions')}>
        <PageSectionContent
          errorMessage={translate('ImportListExclusionsLoadError')}
          isFetching={isFetchingOrHasFetched}
          isPopulated={isPopulated}
          {...otherProps}
        >
          <Table columns={COLUMNS} canModifyColumns={false}
            {...otherProps}
          >
            <TableBody>
              {
                items.map((item, index) => {
                  return (
                    <ImportListExclusion
                      key={item.id}
                      {...item}
                      {...otherProps}
                      index={index}
                      onConfirmDeleteImportListExclusion={onConfirmDeleteImportListExclusion}
                    />
                  );
                })
              }

              <TableRow >
                <TableRowCell />
                <TableRowCell />

                <TableRowCell className={styles.actions}>
                  <IconButton name={icons.ADD} onPress={this.onAddImportListExclusionPress} />
                </TableRowCell>
              </TableRow>
            </TableBody>
          </Table>

          <TablePager
            totalRecords={totalRecords}
            isFetching={isFetching}
            onFirstPagePress={onFirstPagePress}
            {...otherProps}
          />

          <EditImportListExclusionModalConnector
            isOpen={this.state.isAddImportListExclusionModalOpen}
            onModalClose={this.onModalClose}
          />
        </PageSectionContent>
      </FieldSet>
    );
  }
}

ImportListExclusions.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  totalRecords: PropTypes.number,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteImportListExclusion: PropTypes.func.isRequired,
  onFirstPagePress: PropTypes.func.isRequired,
  onSortPress: PropTypes.func.isRequired

};

export default ImportListExclusions;
