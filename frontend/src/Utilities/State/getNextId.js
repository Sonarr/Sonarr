function getNextId(items) {
  return items.reduce((id, x) => Math.max(id, x.id), 1) + 1;
}

export default getNextId;
