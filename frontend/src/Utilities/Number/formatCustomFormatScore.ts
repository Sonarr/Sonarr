function formatCustomFormatScore(
  input?: number,
  customFormatsLength = 0
): string {
  const score = Number(input);

  if (score > 0) {
    return `+${score}`;
  }

  if (score < 0) {
    return `${score}`;
  }

  return customFormatsLength > 0 ? '+0' : '';
}

export default formatCustomFormatScore;
