using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ClientTags.Queries.GetClientTags;

public class GetClientTagsQuery : IRequest<List<ClientTagDto>>
{
}
