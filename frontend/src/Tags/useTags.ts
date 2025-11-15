import { useQueryClient } from '@tanstack/react-query';
import { useMemo, useState } from 'react';
import ModelBase from 'App/ModelBase';
import useApiMutation, {
  getValidationFailures,
} from 'Helpers/Hooks/useApiMutation';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { ValidationFailures } from 'Store/Selectors/selectSettings';
import sortByProp from 'Utilities/Array/sortByProp';

const DEFAULT_TAGS: Tag[] = [];

export interface Tag extends ModelBase {
  label: string;
}

const useTags = () => {
  const { queryKey, ...result } = useApiQuery<Tag[]>({
    path: '/tag',
    queryOptions: {
      gcTime: Infinity,
    },
  });

  return {
    ...result,
    data: result.data ?? DEFAULT_TAGS,
  };
};

export default useTags;

export const useTagList = () => {
  const { data: tags } = useTags();

  return tags;
};

export const useSortedTagList = () => {
  const tagList = useTagList();

  return useMemo(() => {
    return tagList.sort(sortByProp('label'));
  }, [tagList]);
};

export const useAddTag = (onTagCreated?: (tag: Tag) => void) => {
  const queryClient = useQueryClient();
  const [error, setError] = useState<ValidationFailures | null>(null);

  const { mutate, isPending } = useApiMutation<Tag, Pick<Tag, 'label'>>({
    path: '/tag',
    method: 'POST',
    mutationOptions: {
      onMutate: () => {
        setError(null);
      },
      onSuccess: (data) => {
        queryClient.setQueryData<Tag[]>(['tag'], (oldData) => {
          if (!oldData) {
            return oldData;
          }

          return [...oldData, data];
        });

        onTagCreated?.(data);
      },
      onError: (error) => {
        const validationFailures = getValidationFailures(error);

        setError(validationFailures);
      },
    },
  });

  return {
    addTag: mutate,
    isAddingTag: isPending,
    addTagError: error,
  };
};

export const useDeleteTag = (id: number) => {
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);

  const { mutate, isPending } = useApiMutation<Tag, void>({
    path: `/tag/${id}`,
    method: 'DELETE',
    mutationOptions: {
      onMutate: () => {
        setError(null);
      },
      onSuccess: () => {
        queryClient.setQueryData<Tag[]>(['tag'], (oldData) => {
          if (!oldData) {
            return oldData;
          }

          return oldData.filter((tag) => tag.id === id);
        });
      },
      onError: () => {
        setError('Error deleting tag');
      },
    },
  });

  return {
    deleteTag: mutate,
    isDeletingTag: isPending,
    deleteTagError: error,
  };
};
