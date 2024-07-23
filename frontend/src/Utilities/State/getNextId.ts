import ModelBase from 'App/ModelBase';

function getNextId<T extends ModelBase>(items: T[]) {
  return items.reduce((id, x) => Math.max(id, x.id), 1) + 1;
}

export default getNextId;
