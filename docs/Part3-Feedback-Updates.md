# Feedback updates included in Part 3

## Part 1 feedback

| Feedback | Updates in the Smart-X application |
| --- | --- |
| The engagement strategy needed a reason for users to return regularly, not only use the system once. | The Engagement page tracks daily check-ins and consecutive days, prevents duplicate check-ins on the same UTC date, and shows progress. Search events and prior command activity also contribute to the evidence shown with sensor recommendations. |
| The system needed clear Home, About, and Dashboard navigation. | The main navigation includes a Home landing page, an About Smart-X page, and a Dashboard overview, alongside the sensor, telemetry, command, engagement, and topology areas. |
| Use asynchronous work and appropriate try/catch handling. | Command processing runs in the hosted background worker and uses asynchronous delays. The API validates user input, catches and logs errors in file upload, telemetry-range, topology, and command-processing boundaries, and returns safe HTTP errors rather than exposing an exception trace. |

## Part 2 feedback

| Feedback | Updates in the Smart-X application |
| --- | --- |
| Try/catch handling was expected throughout the implementation classes. | The .NET API has a final exception boundary for unexpected requests; API handlers catch and log expected failures; the background command worker catches processing errors; and the new topology and history endpoints handle invalid ranges, missing nodes, and algorithm failures explicitly. The preview API applies corresponding request-boundary handling. |

## Part 3 implementation additions

- Added the Network Topology page and active navigation route.
- Added sensor lookup by ID or MAC address, graph reachability, weighted route results, and a minimum spanning tree view.
- Added AVL-indexed historical telemetry range queries.
- Added explicit busy, error, empty, invalid-range, offline, and unreachable states to the topology workflow.

The network is simulated: positions, links, costs, and latency values are generated for the assignment. They are not measurements from physical radios.
