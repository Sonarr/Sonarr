
function formatPreferredWordScore(input) {
  const score = Number(input);

  if (score > 0) {
    return `+${score}`;
  }

  if (score < 0) {
    return score;
  }

  return '';
}

export default formatPreferredWordScore;
