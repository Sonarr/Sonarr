import ModelBase from 'App/ModelBase';

interface ReleaseProfile extends ModelBase {
  name: string;
  enabled: boolean;
  required: string[];
  ignored: string[];
  indexerId: number;
  tags: number[];
}

export default ReleaseProfile;
