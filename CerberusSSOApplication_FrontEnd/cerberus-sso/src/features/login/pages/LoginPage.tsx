import { Background } from "@/shared/components/background/Background";
import type { JSX } from "react/jsx-runtime";

export const LoginPage = (): JSX.Element => {
  return (
    <main className="relative w-full h-screen overflow-hidden">
      <Background />

      {/* Your login form container */}
      <div className="relative z-10 flex items-center justify-center w-full h-full">
        <div className="p-8 bg-zinc-900/80 backdrop-blur-md rounded-xl border border-zinc-800 text-white">
          <h1 className="text-2xl font-bold mb-4">Cerberus SSO</h1>
          {/* Form inputs go here */}
        </div>
      </div>
    </main>
  );
};
