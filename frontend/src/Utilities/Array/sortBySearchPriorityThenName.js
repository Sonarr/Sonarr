function sortBySearchPriorityThenName(a, b) {
    return (a.fields.find(({name}) => name === 'searchPriority').value - b.fields.find(({name}) => name === 'searchPriority').value) || a.name.localeCompare(b.name);
  }
  
  export default sortBySearchPriorityThenName;