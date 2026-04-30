using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gplant.Domain.DTOs.Responses
{
    public record OrderPagedResult : PagedResult<OrderResponse>
    {
        public int TodayOrderCount { get; init; }  
        public decimal TodayRevenue { get; init; } 
        public int PendingOrderCount { get; init; }  
        public int DeliveringOrderCount { get; init; }
    }
}
