import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./components/**/*.{js,ts,jsx,tsx,mdx}",
    "./app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: "#0F5C6B",
          mid: "#16798C",
          light: "#5BAEBC",
          pale: "#C9E8ED",
        },
        jade: {
          strong: "#0E9F6E",
          mid: "#34C295",
          light: "#8FE3C4",
          pale: "#DFFBEF",
          bg: "#F3FFFA",
        },
        saffron: {
          strong: "#B7791F",
          mid: "#D99A2B",
          light: "#F0C36D",
          pale: "#FBEACB",
          bg: "#FFF8EC",
        },
        periwinkle: {
          strong: "#5469D4",
          mid: "#7B8FE8",
          pale: "#E4E9FC",
          bg: "#F5F7FF",
        },
        plum: {
          strong: "#A1336B",
          mid: "#C2528A",
          pale: "#F8E1EE",
          bg: "#FDF3F8",
        },
        terracotta: {
          strong: "#B6452C",
          mid: "#D06A4A",
          pale: "#FBE6DC",
          bg: "#FFF6F1",
        },
        ink: "#16181D",
        "slate-700": "#3A3F4B",
        "slate-muted": "#5B6472",
        line: "#E3E6EC",
        foot: "#F7F8FA",
        "page-bg": "#EFF1F7",
        surface: "#FFFFFF",
        "surface-dark": "#12141A",
      },
      fontFamily: {
        ui: ["var(--font-manrope)", "system-ui", "sans-serif"],
        display: ["var(--font-sora)", "var(--font-manrope)", "sans-serif"],
      },
      borderRadius: {
        input: "7px",
        button: "9px",
        table: "12px",
        card: "18px",
      },
      boxShadow: {
        dp1: "0 1px 2px rgba(15,23,42,.06), 0 1px 1px rgba(15,23,42,.04)",
        dp2: "0 4px 10px rgba(15,23,42,.10)",
        dp3: "0 12px 28px rgba(15,23,42,.16)",
      },
    },
  },
  plugins: [],
};

export default config;
