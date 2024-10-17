namespace SharpGDX.Ashley.Tests;

public class SystemPriorityTest
{
    public SystemPriorityTest()
    {
        var engine = new Engine();

        engine.addSystem(new SystemA(10));
        engine.addSystem(new SystemB(5));
        engine.addSystem(new SystemA(2));

        engine.update(0);
    }

    public class SystemA : EntitySystem
    {
        public SystemA(int priority)
            : base(priority)
        {
        }

        public override void update(float deltaTime)
        {
            Console.WriteLine("SystemA");
        }
    }

    public class SystemB : EntitySystem
    {
        public SystemB(int priority)
            : base(priority)
        {
        }

        public override void update(float deltaTime)
        {
            Console.WriteLine("SystemB");
        }
    }
}