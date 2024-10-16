namespace SharpGDX.Ashley;

using SharpGDX.Utils;
using SharpGDX.Ashley.Utils;

class SystemManager
{
    private SystemComparator systemComparator = new SystemComparator();
    private Array<EntitySystem> systems = new Array<EntitySystem>(true, 16);
    private ImmutableArray<EntitySystem> immutableSystems ;
    private ObjectMap<Type, EntitySystem> systemsByClass = new ObjectMap<Type, EntitySystem>();
    private SystemListener listener;

    public SystemManager(SystemListener listener)
    {
        immutableSystems = new ImmutableArray<EntitySystem>(systems);
        this.listener = listener;
    }

    public void addSystem(EntitySystem system)
    {
        Type systemType = system.GetType();
        EntitySystem? oldSytem = getSystem<EntitySystem>(systemType);

        if (oldSytem != null)
        {
            removeSystem(oldSytem);
        }

        systems.add(system);
        systemsByClass.put(systemType, system);
        systems.sort(systemComparator);
        listener.systemAdded(system);
    }

    public void removeSystem(EntitySystem system)
    {
        if (systems.removeValue(system, true))
        {
            systemsByClass.remove(system.GetType());
            listener.systemRemoved(system);
        }
    }

    public void removeAllSystems()
    {
        while (systems.size > 0)
        {
            removeSystem(systems.first());
        }
    }

    //@SuppressWarnings("unchecked")
    public T? getSystem<T>(Type systemType)
        where T : EntitySystem
    {
        return (T)systemsByClass.get(systemType);
    }

    public ImmutableArray<EntitySystem> getSystems()
    {
        return immutableSystems;
    }

    private class SystemComparator : IComparer<EntitySystem>
    {
        public int Compare(EntitySystem a, EntitySystem b)
        {
            return a.priority > b.priority ? 1 : (a.priority == b.priority) ? 0 : -1;
        }
    }

    internal interface SystemListener
    {
        void systemAdded(EntitySystem system);
        void systemRemoved(EntitySystem system);
    }
}