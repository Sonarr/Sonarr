import ModelBase from 'App/ModelBase';

function getToggledRange<T extends ModelBase>(
  items: T[],
  id: number | string,
  lastToggled: number | string
) {
  const lastToggledIndex = items.findIndex((item) => item.id === lastToggled);
  const changedIndex = items.findIndex((item) => item.id === id);
  let lower = 0;
  let upper = 0;

  if (lastToggledIndex > changedIndex) {
    lower = changedIndex;
    upper = lastToggledIndex + 1;
  } else {
    lower = lastToggledIndex;
    upper = changedIndex;
  }

  return {
    lower,
    upper,
  };
}

export default getToggledRange;
