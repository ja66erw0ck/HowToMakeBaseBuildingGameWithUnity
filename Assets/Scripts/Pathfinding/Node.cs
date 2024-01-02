namespace Pathfinding
{
    public class Node<T>
    {
        public T Data { get; set; }
        public Edge<T>[] Edges { get; set; }
    }
}
