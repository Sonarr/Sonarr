import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import styles from '../Users.css';

class User extends Component {
  render() {
    const { username } = this.props;

    return (
      <Card className={styles.user}>
        <div className={styles.label}>
          {username}
        </div>
      </Card>
    );
  }
}

User.propTypes = {
  username: PropTypes.string.isRequired
};

export default User;
