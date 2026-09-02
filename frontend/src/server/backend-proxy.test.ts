import { afterEach, describe, expect, it, vi } from "vitest";
import { proxyBackendRequest } from "./backend-proxy";

describe("proxyBackendRequest", () => {
  afterEach(() => {
    vi.restoreAllMocks();
    vi.unstubAllEnvs();
  });

  it("proxies backend responses with query strings", async () => {
    vi.stubEnv("PAYHOOK_API_BASE_URL", "http://api.local");
    const fetchMock = vi.spyOn(globalThis, "fetch").mockResolvedValue(
      new Response(JSON.stringify({ items: [] }), {
        headers: {
          "Content-Type": "application/json; charset=utf-8"
        },
        status: 200
      })
    );

    const response = await proxyBackendRequest("/payments", "?status=Failed");

    expect(fetchMock).toHaveBeenCalledWith(new URL("http://api.local/payments?status=Failed"), {
      cache: "no-store",
      headers: {
        Accept: "application/json"
      }
    });
    expect(response.status).toBe(200);
    expect(response.headers.get("Content-Type")).toBe("application/json; charset=utf-8");
    await expect(response.json()).resolves.toEqual({ items: [] });
  });

  it("returns a bad gateway response when the backend is unavailable", async () => {
    vi.spyOn(globalThis, "fetch").mockRejectedValue(new Error("connection refused"));

    const response = await proxyBackendRequest("/payments");

    expect(response.status).toBe(502);
    await expect(response.json()).resolves.toEqual({
      detail: "connection refused",
      error: "Backend indisponivel"
    });
  });

  it("uses a default content type when the backend omits one", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValue(new Response(new Uint8Array(), { status: 200 }));

    const response = await proxyBackendRequest("/payments");

    expect(response.headers.get("Content-Type")).toBe("application/json");
  });

  it("handles non-error backend exceptions", async () => {
    vi.spyOn(globalThis, "fetch").mockRejectedValue("network failure");

    const response = await proxyBackendRequest("/payments");

    expect(response.status).toBe(502);
    await expect(response.json()).resolves.toEqual({
      detail: "Unexpected proxy error",
      error: "Backend indisponivel"
    });
  });
});
