function titleCase(input) {
  if (!input) {
    return '';
  }

  return input.replace(/\b\w+/g, (match) => {
    return match.charAt(0).toUpperCase() + match.substr(1).toLowerCase();
  });
}

export default titleCase;
