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
    <div> toast </div>
    <div> header </div>
    {children}
    <div> footer </div>
  );
};
