import PropTypes from 'prop-types';
import React, { useEffect } from 'react';
import { useDispatch } from 'react-redux';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import { fetchUsers } from 'Store/Actions/settingsActions';
import translate from 'Utilities/String/translate';
import AddUsers from './AddUsers/AddUsers';
import UserConnector from './User/UserConnector';

function Users(props) {
  const {
    items,
    ...otherProps
  } = props;

  // return (
  //   <PageContent title={translate('Users')}>
  //     <SettingsToolbarConnector
  //       showSave={false}
  //     />
  //     <PageContentBody>
  //       {/* <UsersConnector /> */}
  //     </PageContentBody>
  //   </PageContent>
  // );

  // const userList = useSelector(createUserDetailsSelector());
  const dispatch = useDispatch();
  useEffect(() => {
    dispatch(fetchUsers());
  }, [dispatch]);

  return (
    <PageContent title={translate('Users')}>
      <SettingsToolbarConnector
        showSave={false}
      />
      <PageContentBody>
        {
          items.map((item) => {
            return (
              <UserConnector
                key={item.id}
                {...item}
              />
            );
          })
        }
        <AddUsers />
      </PageContentBody>
    </PageContent>
  );
}

Users.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default Users;
