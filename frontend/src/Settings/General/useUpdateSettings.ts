import useApiQuery from 'Helpers/Hooks/useApiQuery';
import { UpdateMechanism } from 'typings/Settings/General';

interface UpdateSettings {
  branch: string;
  updateAutomatically: boolean;
  updateMechanism: UpdateMechanism;
  updateScriptPath: string;
}

const useUpdateSettings = () => {
  return useApiQuery<UpdateSettings>({
    path: '/settings/update',
  });
};

export default useUpdateSettings;
