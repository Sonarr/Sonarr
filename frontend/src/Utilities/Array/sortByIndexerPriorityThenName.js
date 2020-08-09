function sortByIndexerPriorityThenName(a, b) {
  return (a.priority - b.priority) || a.name.localeCompare(b.name);
}
  
export default sortByIndexerPriorityThenName;
