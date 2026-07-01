import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: "standalone",
  async rewrites() {
    // INTERNAL_API_URL es variable de servidor (sin prefijo NEXT_PUBLIC_).
    // Docker: http://web:8080  |  Local: http://localhost:5000
    const apiUrl = process.env.INTERNAL_API_URL ?? "http://localhost:5000";
    return [
      {
        source: "/api/:path*",
        destination: `${apiUrl}/api/:path*`,
      },
    ];
  },
};

export default nextConfig;
