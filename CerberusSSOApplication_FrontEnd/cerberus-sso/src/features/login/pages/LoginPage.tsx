import type { JSX } from "react/jsx-runtime";

import { LoginForm } from "@/features/login/components/LoginForm";
import { Layout } from "@/shared/components/layout/Layout";
import { Page } from "@/shared/components/page/Page";

import styles from "../components/loginForm.module.scss";

export const LoginPage = (): JSX.Element => {
  return (
    <Layout>
      <Page>
        <Page.Section className={styles.login_page}>
          <LoginForm />
        </Page.Section>
      </Page>
    </Layout>
  );
};
