import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import FilterBuilderRowValue from './FilterBuilderRowValue';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.languageProfiles,
    (languageProfiles) => {
      const tagList = languageProfiles.items.map((languageProfile) => {
        const {
          id,
          name
        } = languageProfile;

        return {
          id,
          name
        };
      });

      return {
        tagList
      };
    }
  );
}

export default connect(createMapStateToProps)(FilterBuilderRowValue);
