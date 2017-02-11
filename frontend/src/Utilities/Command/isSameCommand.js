import _ from 'lodash';

function isSameCommand(commandA, commandB) {
  if (commandA.name.toLocaleLowerCase() !== commandB.name.toLocaleLowerCase()) {
    return false;
  }

  for (const key in commandB) {
    if (key !== 'name') {
      const value = commandB[key];
      if (Array.isArray(value)) {
        if (_.difference(value, commandA[key]).length > 0) {
          return false;
        }
      } else if (value !== commandA[key]) {
        return false;
      }
    }
  }

  return true;
}

export default isSameCommand;
