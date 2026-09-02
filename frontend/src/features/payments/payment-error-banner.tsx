import { XCircle } from "lucide-react";

type PaymentErrorBannerProps = {
  message: string;
};

export function PaymentErrorBanner({ message }: PaymentErrorBannerProps) {
  return (
    <section className="alert-banner error" role="alert">
      <XCircle aria-hidden="true" size={20} />
      <span>{message}</span>
    </section>
  );
}
