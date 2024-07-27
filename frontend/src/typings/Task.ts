import ModelBase from 'App/ModelBase';

interface Task extends ModelBase {
  name: string;
  taskName: string;
  interval: number;
  lastExecution: string;
  lastStartTime: string;
  nextExecution: string;
  lastDuration: string;
}

export default Task;
