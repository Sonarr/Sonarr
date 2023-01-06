interface Column {
  name: string;
  label: string;
  columnLabel: string;
  isSortable: boolean;
  isVisible: boolean;
  isModifiable?: boolean;
}

export default Column;
