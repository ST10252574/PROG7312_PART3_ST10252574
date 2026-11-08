# Part 3 GitHub submission guide

This guide creates 20 non-empty commits that group the supplied project by feature. It does not push anything automatically.

## 1. Clone the empty submission repository

In Visual Studio Developer PowerShell, run:

```powershell
git clone https://github.com/ST10252574/PROG7312_PART3_ST10252574.git
cd PROG7312_PART3_ST10252574
git rev-parse --show-toplevel
git remote -v
```

The printed folder must end in `PROG7312_PART3_ST10252574`, and `origin` must point to the URL above.

## 2. Copy the ZIP contents into the clone

1. Extract `SmartX-Part3-Submission.zip` into a temporary folder.
2. Open that extracted folder and select the items **inside** it (for example `README.md`, `artifacts`, `lib`, `visual-studio`, and `docs`).
3. Copy those items into the cloned `PROG7312_PART3_ST10252574` folder. Do not copy a wrapper folder around them.
4. Keep the clone's hidden `.git` folder. The supplied ZIP does not contain one.
5. Back in PowerShell, confirm `git rev-parse --show-toplevel` still prints the clone path, then run `git status --short`. Do not continue if the project files appear nested inside another `SmartX` folder.

The ZIP excludes `.git`, `.vs`, `bin`, `obj`, `node_modules`, `dist`, generated build caches, local Chromium/config data, screenshots, conversation data, and the earlier Part 1/Part 2 ZIPs. The bundled dashboard in the .NET project's `wwwroot` is included.

## 3. Create 20 feature commits

Run each `git add` and its matching `git commit` in order. Each group should stage new files in an initially empty repository.

```powershell
git add .gitignore .replitignore .replit replit.md .npmrc README.md tsconfig.base.json tsconfig.json
git commit -m "Add repository and editor configuration"

git add package.json pnpm-workspace.yaml pnpm-lock.yaml
git commit -m "Declare workspace packages and dependency versions"

git add scripts
git commit -m "Add repository helper scripts"

git add lib/api-spec
git commit -m "Define the shared Smart-X OpenAPI contract"

git add lib/api-zod
git commit -m "Generate shared API validation schemas and types"

git add lib/api-client-react
git commit -m "Generate the shared React API client"

git add artifacts/api-server/.replit-artifact artifacts/api-server/package.json artifacts/api-server/build.mjs artifacts/api-server/tsconfig.json artifacts/api-server/src/app.ts artifacts/api-server/src/index.ts artifacts/api-server/src/lib/logger.ts artifacts/api-server/src/lib/.gitkeep artifacts/api-server/src/middlewares/.gitkeep artifacts/api-server/src/routes/health.ts artifacts/api-server/src/routes/index.ts
git commit -m "Set up the API server and health routes"

git add artifacts/api-server/src/lib/avl-tree.ts artifacts/api-server/src/lib/binary-min-heap.ts
git commit -m "Add telemetry indexing and priority heap structures"

git add artifacts/api-server/src/lib/mesh-graph.ts artifacts/api-server/src/routes/smartx.ts
git commit -m "Add mesh algorithms and Smart-X API routes"

git add artifacts/mockup-sandbox
git commit -m "Include the workspace component preview artifact"

git add artifacts/smartx-gateway/.replit-artifact artifacts/smartx-gateway/package.json artifacts/smartx-gateway/tsconfig.json artifacts/smartx-gateway/vite.config.ts artifacts/smartx-gateway/index.html artifacts/smartx-gateway/components.json artifacts/smartx-gateway/public
git commit -m "Configure the Smart-X dashboard artifact"

git add artifacts/smartx-gateway/src/App.tsx
git commit -m "Add Smart-X navigation and application pages"

git add artifacts/smartx-gateway/src/components artifacts/smartx-gateway/src/hooks artifacts/smartx-gateway/src/lib artifacts/smartx-gateway/src/pages artifacts/smartx-gateway/src/index.css artifacts/smartx-gateway/src/main.tsx
git commit -m "Add dashboard components and responsive styles"

git add visual-studio/SmartX.Gateway.sln visual-studio/SmartX.Gateway.Api/SmartX.Gateway.Api.csproj visual-studio/SmartX.Gateway.Api/Properties visual-studio/build-dashboard.ps1 visual-studio/README.md
git commit -m "Add the Visual Studio solution and launch setup"

git add visual-studio/SmartX.Gateway.Api/Contracts.cs
git commit -m "Define the .NET Smart-X API contracts"

git add visual-studio/SmartX.Gateway.Api/DataStructures/BasicTree.cs visual-studio/SmartX.Gateway.Api/DataStructures/BinaryTree.cs visual-studio/SmartX.Gateway.Api/DataStructures/BinarySearchTree.cs visual-studio/SmartX.Gateway.Api/DataStructures/AvlTree.cs visual-studio/SmartX.Gateway.Api/DataStructures/RedBlackTree.cs
git commit -m "Implement tree data structures"

git add visual-studio/SmartX.Gateway.Api/DataStructures/BinaryMinHeap.cs visual-studio/SmartX.Gateway.Api/DataStructures/MeshGraph.cs visual-studio/SmartX.Gateway.Api/MeshNetworkService.cs
git commit -m "Implement mesh routing and spanning-tree algorithms"

git add visual-studio/SmartX.Gateway.Api/GatewayStore.cs
git commit -m "Add the in-memory gateway store and history indexes"

git add visual-studio/SmartX.Gateway.Api/Program.cs visual-studio/SmartX.Gateway.Api/CommandProcessor.cs
git commit -m "Add API endpoints and asynchronous command processing"

git add visual-studio/SmartX.Gateway.Api/wwwroot docs
git commit -m "Add the bundled dashboard and submission documents"
```

## 4. Check and push

```powershell
git status --short
git log --oneline -20
git push -u origin HEAD
```

`git status --short` should be empty before pushing. If `git add` reports that a named path does not exist, stop and check that the ZIP contents were copied to the repository root; do not create an empty commit to work around it. If Git asks for an author name or email, configure your own Git identity before committing.
