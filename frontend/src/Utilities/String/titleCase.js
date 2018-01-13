function titleCase(input) {
  if (!input) {
    return '';
  }

  return input.replace(/\w\S*/g, (match) => {
    return match.charAt(0).toUpperCase() + match.substr(1).toLowerCase();
  });
}

export default titleCase;
