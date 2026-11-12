import type { JSX } from "react/jsx-runtime";

import classNames from "classnames";

import { type ICustomizableComponentProps } from "@/shared/types/prop.types";

import styles from "./title.module.scss";

type TitleLevel = "h1" | "h2" | "h3" | "h4" | "h5";

interface ITitleProps extends ICustomizableComponentProps {
  level?: TitleLevel;
  children?: React.ReactNode;
}

export const Title = ({ level = "h1", className, children }: ITitleProps): JSX.Element => {
  const Tag = level;
  return <Tag className={classNames(styles.title, styles[level], className)}>{children}</Tag>;
};
