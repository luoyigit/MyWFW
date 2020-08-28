using System.Threading.Tasks;

namespace EventBus.Abstractions
{
    /// <summary>
    /// 动态EventData的Handler，它并不会要求必须是IntegrationEvent的派生类，可以灵活的处理其它的类型
    /// </summary>
    public interface IDynamicIntegrationEventHandler
    {
        Task Handle(dynamic eventData);
    }
}
