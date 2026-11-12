import type { JSX } from "react/jsx-runtime";
import styles from "./container.module.scss";

interface IContainerProps {
  children: React.ReactNode;
}

export const Container = ({ children }: IContainerProps): JSX.Element => {
  return (
    <div className={styles.container}>
      {children}
    </div>
  );
}