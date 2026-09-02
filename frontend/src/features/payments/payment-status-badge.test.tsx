import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { PaymentStatusBadge } from "./payment-status-badge";

describe("PaymentStatusBadge", () => {
  it.each([
    ["Pending", "Pendente"],
    ["Processed", "Sucesso"],
    ["Failed", "Erro"]
  ] as const)("renders the %s label", (status, label) => {
    render(<PaymentStatusBadge status={status} />);

    expect(screen.getByText(label)).toBeInTheDocument();
  });
});
