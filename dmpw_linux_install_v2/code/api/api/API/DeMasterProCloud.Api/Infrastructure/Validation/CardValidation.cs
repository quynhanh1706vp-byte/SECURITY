using AutoMapper.Configuration;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.User;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>
    /// Card Validation
    /// </summary>
    public class CardValidation : AbstractValidator<CardModel>
    {
        /// <summary>
        /// Card Validation
        /// </summary>
        public CardValidation()
        {
            RuleFor(reg => reg.CardId)
                .NotEmpty()
                .When(reg => reg.CardType == (short)CardType.NFC)
                .WithMessage(MessageResource.CardIdBlank)
                .MaximumLength(40)
                .WithMessage(MessageResource.CardIdLength);

            RuleFor(reg => reg.IssueCount)
                .InclusiveBetween(0, 100)
                .WithMessage(MessageResource.IssueCountMax)
                .Must((reg, c) => (reg.IssueCount.GetType() == 100.GetType()))
                .WithMessage(MessageResource.InvalidIssueCountFormat);

            RuleFor(reg => reg.FingerPrintData)
                .Must((reg, fingerprints) =>
                {
                    if (fingerprints == null || fingerprints.Count == 0)
                    {
                        return true;
                    }
                    else
                    {
                        foreach (var fingerprint in fingerprints)
                        {
                            if (fingerprint.Templates == null || fingerprint.Templates.Count == 0 ||
                                fingerprint.Templates.Count(string.IsNullOrEmpty) > 0)
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                })
                .WithMessage(UserResource.msgDataFingerprintInvalid);
        }
    }
}
