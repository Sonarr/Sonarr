import { createContext, useContext } from 'react';

interface MenuContextProps {
  closeMenu: () => void;
}

const MenuContext = createContext<MenuContextProps | undefined>(undefined);

export function useMenu() {
  return useContext(MenuContext);
}

export default MenuContext;
