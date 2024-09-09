type ArrayElement<V> = V extends (infer U)[] ? U : V;

export default ArrayElement;
