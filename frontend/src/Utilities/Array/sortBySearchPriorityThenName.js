function sortBySearchPriorityThenName(a, b) {
  return (a.priority - b.priority) || a.name.localeCompare(b.name);
}
  
export default sortBySearchPriorityThenName;
