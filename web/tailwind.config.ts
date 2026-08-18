import type { Config } from "tailwindcss";

const color = (name: string) => `rgb(var(--color-${name}) / <alpha-value>)`;

const config: Config = {
  content: [
    "./pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./components/**/*.{js,ts,jsx,tsx,mdx}",
    "./app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        canvas: color("canvas"),
        surface: color("surface"),
        "surface-subtle": color("surface-subtle"),
        "surface-strong": color("surface-strong"),
        field: color("field"),
        foreground: color("foreground"),
        "foreground-secondary": color("foreground-secondary"),
        muted: color("muted"),
        border: color("border"),
        "border-strong": color("border-strong"),
        action: color("action"),
        "action-hover": color("action-hover"),
        "action-soft": color("action-soft"),
        "on-action": color("on-action"),
        accent: color("accent"),
        "accent-soft": color("accent-soft"),
        success: color("success"),
        "success-soft": color("success-soft"),
        warning: color("warning"),
        "warning-soft": color("warning-soft"),
        info: color("info"),
        "info-soft": color("info-soft"),
        danger: color("danger"),
        "danger-soft": color("danger-soft"),
        primary: { DEFAULT: color("action"), mid: color("action-hover"), light: color("action"), pale: color("action-soft") },
        jade: { strong: color("success"), mid: color("success"), light: color("success"), pale: color("success-soft"), bg: color("success-soft") },
        saffron: { strong: color("warning"), mid: color("warning"), light: color("warning"), pale: color("warning-soft"), bg: color("warning-soft") },
        periwinkle: { strong: color("info"), mid: color("info"), pale: color("info-soft"), bg: color("info-soft") },
        plum: { strong: color("accent"), mid: color("accent"), pale: color("accent-soft"), bg: color("accent-soft") },
        terracotta: { strong: color("danger"), mid: color("danger"), pale: color("danger-soft"), bg: color("danger-soft") },
        ink: color("foreground"),
        "slate-700": color("foreground-secondary"),
        "slate-muted": color("muted"),
        line: color("border"),
        foot: color("surface-subtle"),
        "page-bg": color("canvas"),
        "surface-dark": color("surface"),
      },
      fontFamily: {
        ui: ["Inter", "ui-sans-serif", "system-ui", "sans-serif"],
        display: ["Inter", "ui-sans-serif", "system-ui", "sans-serif"],
      },
      borderRadius: { input: "12px", button: "12px", table: "16px", card: "18px", hero: "26px" },
      boxShadow: { dp1: "var(--shadow-1)", dp2: "var(--shadow-2)", dp3: "var(--shadow-3)" },
    },
  },
  plugins: [],
};

export default config;
