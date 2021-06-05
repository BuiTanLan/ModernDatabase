namespace Core.Entities.OrderNeo4j
{
    public class ContainRelationshipNeo4j
    {
        public ContainRelationshipNeo4j()
        {

        }

        public ContainRelationshipNeo4j(int quantity)
        {
            Quantity = quantity;
        }
        //public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
