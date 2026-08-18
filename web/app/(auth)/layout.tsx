export default function AuthLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-dvh bg-canvas p-3 sm:p-5">
      <div className="mx-auto grid min-h-[calc(100dvh-24px)] max-w-[1200px] overflow-hidden rounded-hero border border-border bg-surface shadow-dp2 sm:min-h-[calc(100dvh-40px)] lg:grid-cols-[1.05fr_0.95fr]">
        {children}
      </div>
    </div>
  );
}
