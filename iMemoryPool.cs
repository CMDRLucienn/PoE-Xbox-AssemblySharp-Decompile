public interface iMemoryPool
{
	object GenericAllocate();

	void Free(object obj);
}
