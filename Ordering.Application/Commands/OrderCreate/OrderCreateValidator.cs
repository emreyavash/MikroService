using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Application.Commands.OrderCreate
{
    public class OrderCreateValidator :AbstractValidator<OrderCreateCommand>
    {
        public OrderCreateValidator()
        {
            RuleFor(c => c.SellerUserName).EmailAddress().NotEmpty();
            RuleFor(c => c.ProductId).NotEmpty();
        }
    }
}
