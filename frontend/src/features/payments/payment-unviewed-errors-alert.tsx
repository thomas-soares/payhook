import { AlertTriangle } from "lucide-react";

type PaymentUnviewedErrorsAlertProps = {
  count: number;
};

export function PaymentUnviewedErrorsAlert({ count }: PaymentUnviewedErrorsAlertProps) {
  if (count === 0) {
    return null;
  }

  const label = count === 1 ? "1 erro ainda nao visualizado" : `${count} erros ainda nao visualizados`;

  return (
    <section className="alert-banner warning" role="alert">
      <AlertTriangle aria-hidden="true" size={20} />
      <span>{label}</span>
    </section>
  );
}
