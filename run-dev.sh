#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# run-dev.sh — starts ASP.NET Core + Vite dev servers simultaneously.
#
# Usage:
#   ./run-dev.sh            → MVC sample    (https://localhost:5001, Vite :5173)
#   ./run-dev.sh minimal    → Minimal API   (https://localhost:5002, Vite :5174)
# ─────────────────────────────────────────────────────────────────────────────

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MODE="${1:-mvc}"

if [[ "$MODE" == "minimal" ]]; then
    SERVER_DIR="$SCRIPT_DIR/sample/InertiaSharp.MinimalApi.Sample"
    CLIENT_DIR="$SERVER_DIR/ClientApp"
    VITE_PORT=5174
    DOTNET_URL="https://localhost:5002"
    PROJECT_FILE="InertiaSharp.MinimalApi.Sample.csproj"
    LABEL="Minimal API"
else
    SERVER_DIR="$SCRIPT_DIR/sample/InertiaSharp.Sample"
    CLIENT_DIR="$SERVER_DIR/ClientApp"
    VITE_PORT=5173
    DOTNET_URL="https://localhost:5001"
    PROJECT_FILE="InertiaSharp.Sample.csproj"
    LABEL="MVC Controllers"
fi

# Install npm deps if needed
if [ ! -d "$CLIENT_DIR/node_modules" ]; then
    echo "📦 Installing npm packages..."
    (cd "$CLIENT_DIR" && npm install)
fi

# Restore .NET packages
echo "🔧 Restoring .NET packages..."
dotnet restore "$SCRIPT_DIR/InertiaSharp.sln" --verbosity quiet

echo ""
echo "🚀 Starting [$LABEL] development servers..."
echo "   .NET backend  → $DOTNET_URL"
echo "   Vite frontend → http://localhost:$VITE_PORT"
echo ""
echo "   Visit $DOTNET_URL in your browser."
echo "   Demo credentials: admin@demo.com / Password123!"
echo ""
echo "Press Ctrl+C to stop all servers."
echo ""

cleanup() {
    echo ""
    echo "Shutting down..."
    kill 0
}
trap cleanup INT TERM

# Start Vite dev server on the correct port
(cd "$CLIENT_DIR" && VITE_PORT=$VITE_PORT npm run dev) &

# Start ASP.NET Core with dotnet watch
(cd "$SERVER_DIR" && dotnet watch run \
    --project "$PROJECT_FILE" \
    --launch-profile Development) &

wait -n
cleanup
