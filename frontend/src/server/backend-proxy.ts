import { NextResponse } from "next/server";

const defaultBackendUrl = "http://localhost:5000";

export async function proxyBackendRequest(pathname: string, search = "") {
  const backendUrl = process.env.PAYHOOK_API_BASE_URL ?? defaultBackendUrl;
  const proxyUrl = new URL(pathname, backendUrl);
  proxyUrl.search = search;

  try {
    const response = await fetch(proxyUrl, {
      headers: {
        Accept: "application/json"
      },
      cache: "no-store"
    });

    const body = await response.text();

    return new Response(body, {
      status: response.status,
      headers: {
        "Content-Type": response.headers.get("Content-Type") ?? "application/json"
      }
    });
  } catch (error) {
    const message = error instanceof Error ? error.message : "Unexpected proxy error";

    return NextResponse.json(
      {
        error: "Backend indisponivel",
        detail: message
      },
      { status: 502 }
    );
  }
}
