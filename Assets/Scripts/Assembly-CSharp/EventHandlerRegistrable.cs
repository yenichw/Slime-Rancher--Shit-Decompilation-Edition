public interface EventHandlerRegistrable
{
	void Register(vp_EventHandler eventHandler);

	void Unregister(vp_EventHandler eventHandler);
}
