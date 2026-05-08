using FastEndpoints;
using FluentValidation;
using Org.Shared.Features.Friends;

namespace Org.Backend.Features.Friends.Validators;

public class SendFriendRequestRequestValidator : Validator<SendFriendRequestRequest>
{
    public SendFriendRequestRequestValidator()
    {
        RuleFor(x => x.ReceiverId)
            .NotEmpty().WithMessage("Receiver ID is required");
    }
}
