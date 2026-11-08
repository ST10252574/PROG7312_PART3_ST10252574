# Smart-X in Visual Studio

This folder contains the .NET 10 ASP.NET Core Minimal API project and solution. The dashboard is included as static files under `SmartX.Gateway.Api/wwwroot`, so you can run the whole application from Visual Studio without starting a separate frontend server.

## Install

Install Visual Studio with the **ASP.NET and web development** workload and a **.NET 10 SDK**. Use a Visual Studio release that supports targeting .NET 10. The project has no third-party NuGet dependencies.

If you want to change the React dashboard, also install Node.js 20 or later and pnpm. You do not need Node.js just to run the included dashboard.

## Open and run

1. Extract the project ZIP to a normal folder.
2. Open `visual-studio/SmartX.Gateway.sln` in Visual Studio.
3. Allow Visual Studio to restore/build the solution.
4. Select the `SmartX.Gateway.Api` launch profile and press **F5**.
5. Open `https://localhost:58690/` if the browser does not open automatically. Accept the local development certificate prompt if shown.

The home page is the Smart-X dashboard. The API is available under `/api`, for example:

- `https://localhost:58690/api/healthz`
- `https://localhost:58690/api/summary`
- `https://localhost:58690/api/commands`

## Verify Part 2 and Part 3 features

From the dashboard:

1. Open **Dashboard** and confirm the sensor summary, warnings, recent telemetry, recommendations, and daily operator check-in are visible.
2. Open **Command stream**, submit a pump or relay action, and confirm its status moves from queued to processing to completed.
3. Submit a critical command while a standard command is queued; the critical command is selected first.
4. Use the status/action filters and search field to narrow the command list.
5. Open **Command history**, confirm actions appear newest first, and undo the latest completed manual action. The reversal should enter the critical queue while both commands remain in the audit history.
6. Open **Sensors**, expand a sensor's files, upload a supported file under 10 MiB, then download it from the attachment list.
7. Open **Network topology**. Search by sensor ID or MAC address, inspect the route to the gateway, and switch between BFS and DFS reachability.
8. Compare the highlighted minimum-spanning-tree links and total simulated cost with the full mesh. Offline sensors should be reported as unreachable.
9. Select a sensor and query a historical time range. The matching telemetry should appear in timestamp order.
10. Open **Engagement** and review the strategy and daily check-in progress. A second check-in on the same UTC date does not increase the count.
11. To test telemetry validation, send a request to `POST /api/telemetry` with an unregistered sensor ID or a low moisture value; the API returns validation errors/warnings rather than silently accepting invalid data.
12. Restart the application to confirm the sample data is recreated. This is an in-memory simulation, not persistent storage.

The project intentionally uses simulated devices and seeded data. Recommendations use a transparent history-based heuristic; an operator must choose **Apply** before a recommended command enters the queue. Mesh link costs, routes, and the spanning tree are simulated algorithm results, not measurements from physical radio hardware.

## Rebuild the dashboard after editing it

The bundled `wwwroot` lets the .NET app work by itself. To refresh it after changing the React source:

1. Install Node.js 20+ and pnpm.
2. From the repository root, run `pnpm install`.
3. In PowerShell, run:

   ```powershell
   .\visual-studio\build-dashboard.ps1
   ```

4. Rebuild and run the .NET solution in Visual Studio.

## Assessment data structures

- `Dictionary<string, Sensor>` keyed by MAC address: sensor registration and duplicate detection.
- `SortedDictionary<DateTimeOffset, List<TelemetryReading>>`: time-ordered telemetry, including multiple readings at the same timestamp.
- `AvlTree<DateTimeOffset, TelemetryReading>` per sensor: balanced time index for inclusive range queries in `O(log n + k)`.
- `Queue<DeviceCommand>`: standard commands in first-in, first-out order.
- `PriorityQueue<DeviceCommand, ...>`: critical commands are selected before standard commands.
- `Stack<DeviceCommand>`: newest eligible completed manual action is the undo candidate.
- `BinaryMinHeap<T>`: priority queue used by weighted mesh routing and Prim's minimum spanning tree.
- `MeshGraph` adjacency lists: undirected sensor/relay/gateway connections with BFS, DFS, Dijkstra routing, and Prim MST.
- `GeneralTree`, `BinaryTree`, `BinarySearchTree`, `AvlTree`, and `RedBlackTree`: generic tree implementations covering traversal, ordered lookup, balancing, and range traversal. The per-sensor AVL index is the active time-range query structure.
- `Dictionary<string, Device>`: command-capable device lookup.
- `HashSet<string>`: active alert de-duplication, daily check-in de-duplication, and idempotency tracking.

The command worker uses asynchronous waits and catches/logs unexpected errors. HTTP handlers validate inputs and handle expected failures; a global API exception boundary logs unexpected request errors and returns a safe response.

## Data retention note

The dashboard and APIs simulate the mesh and keep uploaded attachments and runtime state in memory. Restarting the API clears uploads and runtime command/telemetry additions and recreates the seeded example data. Use managed object storage and a persistent database before using this design with real devices.
