import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { Router } from "@/app/routing/Router";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <Router />
  </StrictMode>,
);
