// Queries/GetOrdersByUserQuery.cs
public record GetOrdersByUserQuery : IRequest<IEnumerable<OrderDto>>
{
    public required Guid UserId { get; init; }
    public int PageSize { get; init; } = 10;
    public int PageNumber { get; init; } = 1;
}