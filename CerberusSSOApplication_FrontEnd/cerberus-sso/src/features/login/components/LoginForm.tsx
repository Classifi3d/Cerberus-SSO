import { type FormEvent, useState } from "react";
import type { JSX } from "react/jsx-runtime";

import { Title } from "@/shared/components/title/Title";
import { Button } from "@/shared/ui/button";
import { Field, FieldLabel } from "@/shared/ui/field";
import { Input } from "@/shared/ui/input";

import styles from "./loginForm.module.scss";

export const LoginForm = (): JSX.Element => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const handleSubmit = (event: FormEvent<HTMLFormElement>): void => {
    event.preventDefault();
  };

  return (
    <div className={styles.login_card}>
      <Title className={styles.title}>Sign In</Title>
      <form className={styles.form} onSubmit={handleSubmit}>
        <Field>
          <FieldLabel htmlFor="email">Email</FieldLabel>
          <Input
            id="email"
            type="email"
            name="email"
            autoComplete="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="user@mail.com"
            required
          />
        </Field>
        <Field>
          <FieldLabel htmlFor="password">Password</FieldLabel>
          <Input
            id="password"
            type="password"
            name="password"
            autoComplete="current-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="P@s5w0rd"
            required
          />
        </Field>
        <Button type="submit">Next</Button>
      </form>
    </div>
  );
};
