import { BinaryMinHeap } from "./binary-min-heap";

export type MeshNode = {
  id: string;
  label: string;
  type: "gateway" | "router" | "sensor";
  macAddress: string | null;
  status: "online" | "warning" | "offline";
  x: number;
  y: number;
};

export type MeshLink = {
  fromId: string;
  toId: string;
  cost: number;
  latencyMs: number;
};

type WeightedNode = { id: string; distance: number };
type CandidateEdge = { edge: MeshLink; targetId: string };

const compareWeightedNodes = (left: WeightedNode, right: WeightedNode) =>
  left.distance - right.distance || left.id.localeCompare(right.id);
const compareCandidateEdges = (left: CandidateEdge, right: CandidateEdge) =>
  left.edge.cost - right.edge.cost ||
  left.edge.fromId.localeCompare(right.edge.fromId) ||
  left.edge.toId.localeCompare(right.edge.toId);

export class MeshGraph {
  private readonly nodes = new Map<string, MeshNode>();
  private readonly adjacency = new Map<string, MeshLink[]>();
  private readonly links: MeshLink[] = [];
  private readonly linkKeys = new Set<string>();

  addNode(node: MeshNode): void {
    if (!node.id.trim() || this.nodes.has(node.id)) {
      throw new Error(`Mesh node '${node.id}' is invalid or already registered.`);
    }
    this.nodes.set(node.id, node);
    this.adjacency.set(node.id, []);
  }

  addLink(link: MeshLink): void {
    if (
      !this.nodes.has(link.fromId) ||
      !this.nodes.has(link.toId) ||
      link.fromId === link.toId ||
      !Number.isFinite(link.cost) ||
      link.cost <= 0 ||
      !Number.isFinite(link.latencyMs) ||
      link.latencyMs < 0
    ) {
      throw new Error("Mesh connections must reference two distinct nodes and have valid costs.");
    }
    const key = this.linkKey(link.fromId, link.toId);
    if (this.linkKeys.has(key)) return;
    this.linkKeys.add(key);
    this.links.push(link);
    this.adjacency.get(link.fromId)?.push(link);
    this.adjacency.get(link.toId)?.push({
      fromId: link.toId,
      toId: link.fromId,
      cost: link.cost,
      latencyMs: link.latencyMs,
    });
  }

  allNodes(): MeshNode[] {
    return [...this.nodes.values()];
  }

  allLinks(): MeshLink[] {
    return this.links.map((link) => ({ ...link }));
  }

  shortestCostPath(startId: string, destinationId: string): {
    reachable: boolean;
    path: string[];
    hops: number;
    totalCost: number;
  } {
    const start = this.nodes.get(startId);
    const destination = this.nodes.get(destinationId);
    if (!start || !destination || start.status === "offline" || destination.status === "offline") {
      return { reachable: false, path: [], hops: 0, totalCost: 0 };
    }

    const distance = new Map<string, number>([[startId, 0]]);
    const previous = new Map<string, string>();
    const pending = new BinaryMinHeap(compareWeightedNodes);
    pending.push({ id: startId, distance: 0 });

    while (pending.size > 0) {
      const current = pending.pop();
      if (!current) break;
      if (current.distance !== distance.get(current.id)) continue;
      if (current.id === destinationId) break;
      for (const edge of this.sortedEdges(current.id)) {
        const neighbor = this.nodes.get(edge.toId);
        if (!neighbor || neighbor.status === "offline") continue;
        const candidateDistance = current.distance + edge.cost;
        if (candidateDistance >= (distance.get(neighbor.id) ?? Number.POSITIVE_INFINITY)) continue;
        distance.set(neighbor.id, candidateDistance);
        previous.set(neighbor.id, current.id);
        pending.push({ id: neighbor.id, distance: candidateDistance });
      }
    }

    const totalCost = distance.get(destinationId);
    if (totalCost === undefined) return { reachable: false, path: [], hops: 0, totalCost: 0 };
    const path = [destinationId];
    while (path[0] !== startId) {
      const prior = previous.get(path[0]);
      if (!prior) return { reachable: false, path: [], hops: 0, totalCost: 0 };
      path.unshift(prior);
    }
    return { reachable: true, path, hops: Math.max(0, path.length - 1), totalCost };
  }

  traverse(startId: string, mode: "bfs" | "dfs" = "bfs"): string[] {
    const start = this.nodes.get(startId);
    if (!start || start.status === "offline") return [];
    const visited = new Set<string>([startId]);
    const order: string[] = [];
    const work = [startId];
    while (work.length > 0) {
      const current = mode === "bfs" ? work.shift() : work.pop();
      if (!current) continue;
      order.push(current);
      const neighbors = this.sortedEdges(current)
        .map((edge) => edge.toId)
        .filter((id) => this.nodes.get(id)?.status !== "offline" && !visited.has(id));
      if (mode === "dfs") neighbors.reverse();
      for (const neighbor of neighbors) {
        visited.add(neighbor);
        work.push(neighbor);
      }
    }
    return order;
  }

  minimumSpanningTree(startId: string): { links: MeshLink[]; totalCost: number } {
    const activeIds = [...this.nodes.values()]
      .filter((node) => node.status !== "offline")
      .map((node) => node.id)
      .sort();
    if (!this.nodes.has(startId) || this.nodes.get(startId)?.status === "offline") {
      return { links: [], totalCost: 0 };
    }

    const visited = new Set<string>();
    const selected: MeshLink[] = [];
    const candidates = new BinaryMinHeap(compareCandidateEdges);
    const addAvailableEdges = (nodeId: string) => {
      for (const edge of this.sortedEdges(nodeId)) {
        if (this.nodes.get(edge.toId)?.status !== "offline" && !visited.has(edge.toId)) {
          candidates.push({ edge, targetId: edge.toId });
        }
      }
    };

    const starts = [startId, ...activeIds.filter((id) => id !== startId)];
    for (const rootId of starts) {
      if (visited.has(rootId)) continue;
      visited.add(rootId);
      addAvailableEdges(rootId);
      while (candidates.size > 0) {
        const candidate = candidates.pop();
        if (!candidate || visited.has(candidate.targetId)) continue;
        visited.add(candidate.targetId);
        selected.push(candidate.edge);
        addAvailableEdges(candidate.targetId);
      }
    }
    return {
      links: selected.map(({ fromId, toId, cost, latencyMs }) => ({ fromId, toId, cost, latencyMs })),
      totalCost: selected.reduce((sum, edge) => sum + edge.cost, 0),
    };
  }

  private sortedEdges(nodeId: string): MeshLink[] {
    return [...(this.adjacency.get(nodeId) ?? [])].sort(
      (left, right) => left.cost - right.cost || left.toId.localeCompare(right.toId),
    );
  }

  private linkKey(first: string, second: string): string {
    return [first, second].sort().join("::");
  }
}
