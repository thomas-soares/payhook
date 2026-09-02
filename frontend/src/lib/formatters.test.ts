import { describe, expect, it } from "vitest";
import { formatAmount, formatDate, formatTime } from "./formatters";

describe("formatters", () => {
  it("formats nullable amounts", () => {
    expect(formatAmount(null)).toBe("--");
    expect(formatAmount(199.9)).toContain("199,90");
  });

  it("formats dates and times for Brazilian Portuguese", () => {
    expect(formatDate("2026-09-01T18:00:00Z")).toMatch(/01\/09\/2026/);
    expect(formatTime(Date.UTC(2026, 8, 1, 18, 0, 0))).toMatch(/\d{2}:\d{2}:\d{2}/);
  });
});
