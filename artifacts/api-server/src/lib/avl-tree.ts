type AvlNode<TKey, TValue> = {
  key: TKey;
  values: TValue[];
  height: number;
  left: AvlNode<TKey, TValue> | null;
  right: AvlNode<TKey, TValue> | null;
};

export class AvlTree<TKey, TValue> {
  private root: AvlNode<TKey, TValue> | null = null;
  private valueCount = 0;

  constructor(private readonly compare: (left: TKey, right: TKey) => number) {}

  get size(): number {
    return this.valueCount;
  }

  insert(key: TKey, value: TValue): void {
    this.root = this.insertNode(this.root, key, value);
    this.valueCount += 1;
  }

  range(minimum: TKey, maximum: TKey): TValue[] {
    if (this.compare(minimum, maximum) > 0) {
      throw new RangeError("The range start must not be later than the range end.");
    }
    const result: TValue[] = [];
    this.collectRange(this.root, minimum, maximum, result);
    return result;
  }

  private insertNode(
    node: AvlNode<TKey, TValue> | null,
    key: TKey,
    value: TValue,
  ): AvlNode<TKey, TValue> {
    if (!node) return { key, values: [value], height: 1, left: null, right: null };

    const order = this.compare(key, node.key);
    if (order === 0) {
      node.values.push(value);
      return node;
    }
    if (order < 0) node.left = this.insertNode(node.left, key, value);
    else node.right = this.insertNode(node.right, key, value);

    this.updateHeight(node);
    return this.rebalance(node);
  }

  private collectRange(
    node: AvlNode<TKey, TValue> | null,
    minimum: TKey,
    maximum: TKey,
    result: TValue[],
  ): void {
    if (!node) return;
    const lowerOrder = this.compare(node.key, minimum);
    const upperOrder = this.compare(node.key, maximum);
    if (lowerOrder > 0) this.collectRange(node.left, minimum, maximum, result);
    if (lowerOrder >= 0 && upperOrder <= 0) result.push(...node.values);
    if (upperOrder < 0) this.collectRange(node.right, minimum, maximum, result);
  }

  private height(node: AvlNode<TKey, TValue> | null): number {
    return node?.height ?? 0;
  }

  private balance(node: AvlNode<TKey, TValue>): number {
    return this.height(node.left) - this.height(node.right);
  }

  private updateHeight(node: AvlNode<TKey, TValue>): void {
    node.height = 1 + Math.max(this.height(node.left), this.height(node.right));
  }

  private rotateRight(root: AvlNode<TKey, TValue>): AvlNode<TKey, TValue> {
    const pivot = root.left;
    if (!pivot) return root;
    root.left = pivot.right;
    pivot.right = root;
    this.updateHeight(root);
    this.updateHeight(pivot);
    return pivot;
  }

  private rotateLeft(root: AvlNode<TKey, TValue>): AvlNode<TKey, TValue> {
    const pivot = root.right;
    if (!pivot) return root;
    root.right = pivot.left;
    pivot.left = root;
    this.updateHeight(root);
    this.updateHeight(pivot);
    return pivot;
  }

  private rebalance(node: AvlNode<TKey, TValue>): AvlNode<TKey, TValue> {
    const skew = this.balance(node);
    if (skew > 1) {
      if (node.left && this.balance(node.left) < 0) node.left = this.rotateLeft(node.left);
      return this.rotateRight(node);
    }
    if (skew < -1) {
      if (node.right && this.balance(node.right) > 0) node.right = this.rotateRight(node.right);
      return this.rotateLeft(node);
    }
    return node;
  }
}
