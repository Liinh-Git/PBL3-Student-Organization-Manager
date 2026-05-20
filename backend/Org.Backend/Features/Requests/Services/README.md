# Requests Services

## IRequestService / RequestService
**Methods**:
- `Task<List<RequestDto>> ListRequestsAsync(Guid orgId, Guid userId)`
- `Task<RequestDto> SubmitRequestAsync(Guid orgId, SubmitRequestRequest request, Guid userId)`
- `Task<RequestDto> GetRequestAsync(Guid requestId, Guid userId)`
- `Task<RequestDto> ReviewRequestAsync(Guid requestId, ReviewRequestRequest request, Guid userId)`

## NOT Implemented in Phase 3C
- ❌ No real service implementations
