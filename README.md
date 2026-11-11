# Smart-X IoT Mesh Gateway — Parts 1–3

Smart-X is a simulated IoT gateway for validating telemetry and managing command-capable devices. The application includes a React dashboard and an ASP.NET Core Minimal API package for Visual Studio. Sample readings and devices are simulated in memory; no physical sensors are required.

## Run in Visual Studio

1. Install Visual Studio with the **ASP.NET and web development** workload and a **.NET 10 SDK**. Use a Visual Studio release that supports .NET 10.
2. Extract the ZIP to a folder.
3. Open `visual-studio/SmartX.Gateway.sln`.
4. Build, then press **F5** using the `SmartX.Gateway.Api` profile.
5. Browse to `https://localhost:58690/` for the dashboard. If it is not opened automatically, use the address directly.

The API runs on the same host under `/api`. For example, visit `/api/healthz`, `/api/summary`, or `/api/commands`.

The extracted ZIP includes the built dashboard. Node.js and pnpm are only needed if you plan to edit/rebuild the React dashboard.

## Run the preview implementation

Requirements: Node.js 20+ and pnpm.

```sh
pnpm install
pnpm --filter @workspace/api-server run dev
pnpm --filter @workspace/smartx-gateway run dev
```

The preview implementation uses a Node API server and Vite dashboard. The separate Visual Studio package is the .NET 10 submission version.

## Part 2 and Part 3 features

- Home, About, Dashboard, sensor, telemetry, command stream, command history, and engagement pages with responsive navigation.
- Asynchronous pump and relay command processing with standard FIFO and critical-priority queues.
- Command search and status/action filters; history is retained newest-first.
- Undo for the latest completed manual command. The inverse is added to the critical queue, and both actions remain in the audit history.
- Sensor profile attachments for configuration files, logs, CSV/JSON/XML, PDF, and JPG/PNG/WEBP images (10 MiB per file).
- Device controls for simulated pumps and relays.
- Explicit search-event tracking and daily review check-ins to support repeat operator engagement.
- Evidence-based recommendations using low-moisture telemetry, prior commands, and submitted device searches. Recommendations are advisory: applying one is required before a device command is queued.
- A live mesh topology page with sensor/MAC route lookup, weighted route calculation, BFS/DFS reachability, offline-node detection, and a Prim minimum spanning tree.
- Historical sensor range queries backed by per-sensor AVL indexes rather than a full telemetry-list scan.
- Try/catch handling at API and background-processing boundaries, with visible loading, retry, empty, and error states.

## Build checks

From the repository root:

```sh
pnpm install
pnpm run typecheck
dotnet build visual-studio/SmartX.Gateway.sln
```

The dashboard's production bundle is copied into the .NET project under `wwwroot`; rebuild it after changing React source by running `.\visual-studio\build-dashboard.ps1` in PowerShell.

## Test checklist

With the .NET API running:

1. `GET /api/healthz` should return `{"status":"ok"}`.
2. `GET /api/summary`, `/api/sensors`, `/api/telemetry`, and `/api/devices` should return seeded simulation data.
3. In the dashboard, submit a pump or relay command. It should progress asynchronously through queued, processing, and completed.
4. Apply a recommendation and confirm the resulting command appears in the command stream and history.
5. Open **Command history** and use **Undo latest** on the newest completed manual action. Confirm its inverse enters the critical queue and both records remain in history.
6. Open **Sensors**, expand a sensor's files, upload an allowed file under 10 MiB, then download it from the attachment list.
7. Submit a search in the command page. Search history is listed at `GET /api/searches` and influences the explanatory recommendation evidence.
8. Complete the daily operator review once; a repeat submission during the same UTC day is idempotent.
9. `POST /api/telemetry` with an invalid sensor ID should return a validation result and not add a reading.
10. Open **Network topology**, search for `sensor-001` or a registered MAC address, and inspect the weighted route to the gateway.
11. Switch between BFS and DFS connectivity; the offline Pretoria sensor should appear as unreachable.
12. Query a historical time range for a selected sensor and confirm matching readings are ordered by timestamp.
13. Compare the highlighted minimum-spanning-tree links and total simulated cost with the full mesh.

The gateway store resets to its seeded sample data whenever the application restarts. Uploaded attachments and command undo history are also held in memory and reset on restart. This matches the assignment's simulation requirement and is not intended as persistent production storage.

## Rebuild the bundled dashboard

The React source lives in `artifacts/smartx-gateway`; the .NET app serves the built copy from `visual-studio/SmartX.Gateway.Api/wwwroot`.

From the repository root, after installing Node.js 20+ and pnpm:

```powershell
pnpm install
.\visual-studio\build-dashboard.ps1
```

Then rebuild and run the solution from Visual Studio.

## Project map

- `visual-studio/` — Visual Studio solution, .NET API, background command worker, launch profile, and dashboard bundling script.
- `artifacts/smartx-gateway/` — React/Vite dashboard source.
- `artifacts/api-server/` — Node-based API used for the Replit preview.
- `lib/api-spec/` — OpenAPI contract shared by the generated API client and Zod schemas.
- `visual-studio/SmartX.Gateway.Api/DataStructures/` — tree, heap, graph traversal, weighted routing, and minimum-spanning-tree implementations.
- `docs/part1-research.md` and `docs/Part1-Research-SmartX.docx` — Part 1 research deliverables.
- `docs/Part3-Implementation-Report.pdf` — Part 3 implementation report and technology recommendations.
- `docs/Part3-Feedback-Updates.md` — changes made in response to the Part 1 and Part 2 feedback.
- `docs/Part3-Demo-Script.md` — outline for recording the required application walkthrough.
- `docs/Part3-Git-Push-Guide.md` — safe ZIP extraction and 20 meaningful PowerShell commit steps for the empty submission repository.

For the Visual Studio setup and test steps, see [`visual-studio/README.md`](visual-studio/README.md).

## AI Tool Disclosure -- Claude

- Claude Ai was used to debug and fault find errors with code. All final decisions were made by Sabein Naidoo
