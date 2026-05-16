import { Failure, ValidationFailure } from 'typings/pending';

const ITEM_REGEX = /^Items\[(\d+)\](?:\.Items\[(\d+)\])?\.(\w+)$/i;

export type SizeField = 'minSize' | 'maxSize' | 'preferredSize';

export interface SizeFailures {
  minSize: Failure[];
  maxSize: Failure[];
  preferredSize: Failure[];
}

export interface ItemFailures {
  errors: SizeFailures;
  warnings: SizeFailures;
}

export type ItemFailuresMap = Map<string, ItemFailures>;

function emptySizeFailures(): SizeFailures {
  return { minSize: [], maxSize: [], preferredSize: [] };
}

export function emptyItemFailures(): ItemFailures {
  return { errors: emptySizeFailures(), warnings: emptySizeFailures() };
}

function toFailure(failure: ValidationFailure): Failure {
  return {
    errorMessage: failure.errorMessage,
    infoLink: failure.infoLink,
    detailedDescription: failure.detailedDescription,

    message: failure.errorMessage,
    link: failure.infoLink,
    detailedMessage: failure.detailedDescription,
  };
}

function toSizeField(field: string): SizeField | null {
  switch (field.toLowerCase()) {
    case 'minsize':
      return 'minSize';
    case 'maxsize':
      return 'maxSize';
    case 'preferredsize':
      return 'preferredSize';
    default:
      return null;
  }
}

export function itemFailuresKey(
  itemIndex: number,
  groupIndex: number | null = null
): string {
  return groupIndex == null ? `${itemIndex}` : `${itemIndex}.${groupIndex}`;
}

function addFailures(
  result: ItemFailuresMap,
  failures: ValidationFailure[],
  target: 'errors' | 'warnings'
) {
  failures.forEach((failure) => {
    const match = failure.propertyName.match(ITEM_REGEX);

    if (!match) {
      return;
    }

    const field = toSizeField(match[3]);

    if (!field) {
      return;
    }

    const itemIndex = parseInt(match[1]);
    const groupIndex = match[2] == null ? null : parseInt(match[2]);
    const key = itemFailuresKey(itemIndex, groupIndex);

    let entry = result.get(key);

    if (!entry) {
      entry = emptyItemFailures();
      result.set(key, entry);
    }

    entry[target][field].push(toFailure(failure));
  });
}

export function parseItemFailures(
  errors: ValidationFailure[],
  warnings: ValidationFailure[]
): ItemFailuresMap {
  const result = new Map<string, ItemFailures>();

  addFailures(result, errors, 'errors');
  addFailures(result, warnings, 'warnings');

  return result;
}

export function getItemFailures(
  map: ItemFailuresMap,
  itemIndex: number,
  groupIndex: number | null = null
): ItemFailures {
  return map.get(itemFailuresKey(itemIndex, groupIndex)) ?? emptyItemFailures();
}
