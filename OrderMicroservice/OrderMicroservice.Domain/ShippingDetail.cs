namespace OrderMicroservice.Domain{
public class ShippingDetail
    {
        public required string Method { get; set; }      // e.g., "BLUEDART_EXPRESS" (kept as string per your data)
        public DateOnly PromisedDate { get; set; }       // "YYYY-MM-DD" in your JSON
    }
}
 