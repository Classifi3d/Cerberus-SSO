import type { JSX } from "react/jsx-runtime";

import { Spinner } from "@/shared/ui/spinner";

import styles from "./spinner.module.scss";

export const SpinnerContainer = (): JSX.Element => (
  <div className={styles.spinner}>
    <Spinner />
  </div>
);
