import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";
import { fileURLToPath } from "node:url";

export default defineConfig({
  plugins: [react()],
  test: {
    coverage: {
      exclude: [
        ".next/**",
        "src/app/**",
        "src/test/**",
        "src/**/test-data.ts",
        "src/**/*.test.ts",
        "src/**/*.test.tsx",
        "src/**/*.d.ts"
      ],
      include: ["src/components/**/*.{ts,tsx}", "src/features/**/*.{ts,tsx}", "src/lib/**/*.{ts,tsx}"],
      provider: "v8",
      reporter: ["text", "html", "lcov"]
    },
    environment: "jsdom",
    globals: true,
    setupFiles: ["./src/test/setup.ts"]
  },
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url))
    }
  }
});
