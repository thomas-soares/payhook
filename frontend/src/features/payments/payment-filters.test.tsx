import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { PaymentFilters } from "./payment-filters";

describe("PaymentFilters", () => {
  it("marks the selected status and emits status changes", () => {
    const onChange = vi.fn();

    render(<PaymentFilters filters={{ contractId: "", status: "all" }} onChange={onChange} />);

    expect(screen.getByRole("button", { name: "Todos" })).toHaveAttribute("aria-pressed", "true");

    fireEvent.click(screen.getByRole("button", { name: "Erro" }));

    expect(onChange).toHaveBeenCalledWith({ contractId: "", status: "Failed" });
  });

  it("emits contract filter changes", () => {
    const onChange = vi.fn();

    render(<PaymentFilters filters={{ contractId: "", status: "Processed" }} onChange={onChange} />);

    fireEvent.change(screen.getByPlaceholderText("ID do contrato"), {
      target: { value: "contract-010" }
    });

    expect(onChange).toHaveBeenCalledWith({ contractId: "contract-010", status: "Processed" });
  });
});
