export interface ICustomizableComponentProps {
  className?: string;
}

export interface ITypographyComponentProps extends ICustomizableComponentProps {
  text?: string;
  size?: "large" | "default" | "small" | "extra_small";
  children?: React.ReactNode;
}

export interface IQueryProps {
  enabled?: boolean;
  alwaysRefetch?: boolean;
}
