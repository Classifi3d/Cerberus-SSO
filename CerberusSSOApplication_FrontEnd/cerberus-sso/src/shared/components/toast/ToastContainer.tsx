import type { JSX } from "react/jsx-runtime";

import { Toaster } from "@/shared/ui/toast";

export const ToastContainer = (): JSX.Element => {
  return (
    <div>
      <Toaster />
    </div>
  );
};
