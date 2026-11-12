import type { PropsWithChildren } from "react";
import type { JSX } from "react/jsx-runtime";

import classNames from "classnames";

import { type ICustomizableComponentProps } from "@/shared/types/prop.types";

import styles from "./page.module.scss";

export const Page = ({
  className,
  children,
}: PropsWithChildren & ICustomizableComponentProps): JSX.Element => {
  return <div className={className}>{children}</div>;
};

interface ISectionProps {
  bordered?: boolean;
  padded?: boolean;
}

const Section = ({
  className,
  children,
  bordered,
  padded = false,
}: PropsWithChildren & ICustomizableComponentProps & ISectionProps) => {
  return (
    <div
      className={classNames(
        styles.page_section,
        { [styles.border]: bordered, [styles.padded]: padded },
        className,
      )}
    >
      {children}
    </div>
  );
};

Page.Section = Section;
