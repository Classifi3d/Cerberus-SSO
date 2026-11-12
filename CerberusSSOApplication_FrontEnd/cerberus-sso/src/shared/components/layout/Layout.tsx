import type { JSX, ReactNode } from "react";
import { When } from "react-if";

import { Header } from "@/shared/components/layout/header/Header";

import { Background } from "../background/Background";
import { ToastContainer } from "../toast/ToastContainer";
import { Footer } from "./footer/Footer";
import styles from "./layout.module.scss";

interface ILayoutProps {
  hasBackground?: boolean;
  hasHeader?: boolean;
  hasFooter?: boolean;
  isLoading?: boolean;
  children: ReactNode;
}

export const Layout = ({
  hasBackground = true,
  hasHeader = true,
  hasFooter = true,
  isLoading = false,
  children,
}: ILayoutProps): JSX.Element => {
  return (
    <div className={styles.layout}>
      <When condition={hasBackground}>
        <Background />
      </When>
      <ToastContainer />
      <When condition={hasHeader}>
        <Header />
      </When>
      {children}
      <When condition={hasFooter}>
        <Footer />
      </When>
    </div>
  );
};
