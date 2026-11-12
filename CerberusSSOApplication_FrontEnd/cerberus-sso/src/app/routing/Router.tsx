import React from "react";
import { BrowserRouter, Route, Routes } from "react-router";

import { Layout } from "@/shared/components/layout/Layout";
import { SpinnerContainer } from "@/shared/components/spinner/SpinnerContainer";

const routes = {
  login: "/login",
  loginMfa: "login/mfa",
  signup: "",
  account: "",
  accountMfaEnrollment: "/account/mfa-enrollment",
};

const LoginPage = React.lazy(() =>
  import("@/features/login/pages/LoginPage").then((module) => ({ default: module.LoginPage })),
);

export const Router = () => (
  <BrowserRouter>
    <React.Suspense
      fallback={
        <Layout>
          <SpinnerContainer />
        </Layout>
      }
    >
      <Routes>
        <Route path={routes.login} element={<LoginPage />} />
        <Route path={routes.loginMfa} element={<LoginPage />} />
        <Route path={routes.account} element={<LoginPage />} />
      </Routes>
    </React.Suspense>
  </BrowserRouter>
);
