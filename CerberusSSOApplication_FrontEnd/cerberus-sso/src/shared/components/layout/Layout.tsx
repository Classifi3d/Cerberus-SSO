import { When } from "react-if";
import type { ReactNode } from "react";

interface ILayoutProps {
  simplified?: boolean;
  isLoading?: boolean;
  children: ReactNode;
}

export const Layout = ({
  simplified = false,
  isLoading = false,
  children,
}: ILayoutProps) => {
  return (
    <div>
      <div> toast </div>
      <When condition={!simplified}>
        <div> header </div>
      </When>
      {children}
      <When condition={!isLoading}>
        <div> footer </div>
      </When>
    </div>
  );
};
