import type { NextRequest } from "next/server";
import { proxyBackendRequest } from "@/server/backend-proxy";

export const dynamic = "force-dynamic";

export async function GET(request: NextRequest) {
  return proxyBackendRequest("/payments", request.nextUrl.search);
}
