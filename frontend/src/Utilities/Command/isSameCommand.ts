import { CommandBody } from 'Commands/Command';

function isSameCommand(
  commandA: Partial<CommandBody>,
  commandB: Partial<CommandBody>
): boolean {
  if (commandA.name?.toLowerCase() !== commandB.name?.toLowerCase()) {
    return false;
  }

  for (const key in commandB) {
    if (key !== 'name') {
      const valueB = commandB[key as keyof CommandBody];
      const valueA = commandA[key as keyof CommandBody];

      if (Array.isArray(valueB)) {
        if (!Array.isArray(valueA)) {
          return false;
        }

        // Sort both arrays for comparison
        const sortedB = [...(valueB as number[])].sort((a, b) => a - b);
        const sortedA = [...(valueA as number[])].sort((a, b) => a - b);

        if (sortedA.length !== sortedB.length) {
          return false;
        }

        for (let i = 0; i < sortedB.length; i++) {
          if (sortedB[i] !== sortedA[i]) {
            return false;
          }
        }
      } else if (valueB !== valueA) {
        return false;
      }
    }
  }

  return true;
}

export default isSameCommand;
