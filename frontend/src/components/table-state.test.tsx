import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { TableState } from "./table-state";

describe("TableState", () => {
  it("shows the loading label as a status", () => {
    render(<TableState emptyLabel="Nenhum item" isLoading={true} loadingLabel="Carregando..." />);

    expect(screen.getByRole("status")).toHaveTextContent("Carregando...");
  });

  it("shows the empty label when not loading", () => {
    render(<TableState emptyLabel="Nenhum item" isLoading={false} loadingLabel="Carregando..." />);

    expect(screen.getByText("Nenhum item")).toBeInTheDocument();
  });
});
