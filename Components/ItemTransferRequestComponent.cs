using System;
using CitySim.Data;
using CitySim.Data.StateEffects;
using CitySim.ECS;

namespace CitySim.Components
{
    public class ItemTransferRequestComponent : IComponent
    {
        public required Item Item { get; set; }
        public required float Amount { get; set; }
        public decimal? Cost { get; set; }
        public required Guid SourceEntityID { get; set; }
        public IStateEffect[]? OnCompleteEffects {get;set;} =  null;
    }
}
