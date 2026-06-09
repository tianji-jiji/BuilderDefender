public interface IPoolable
{
    // 处理对象从对象池取出后的状态重置。
    void OnSpawned();

    // 处理对象回收到对象池前的状态清理。
    void OnDespawned();
}
